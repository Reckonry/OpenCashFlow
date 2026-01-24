using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using global::Shared.Models;
using global::Shared.Models.Companies;
using global::Shared.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Middleware
{
    public class SubscriptionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public SubscriptionAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuthenticationService authService,
            IBillingRepository billingRepository,
            IOptions<SubscriptionAuthorizationOptions> optionsAccessor,
            ILogger<SubscriptionAuthorizationMiddleware> logger)
        {
            var options = optionsAccessor.Value;

            if (!options.Enabled || HttpMethods.Options.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!(context.User?.Identity?.IsAuthenticated ?? false))
            {
                await _next(context);
                return;
            }

            if (IsBypassedPath(context.Request.Path, options.BypassPaths))
            {
                await _next(context);
                return;
            }

            if (IsUserInAdminRole(context.User, options.AdminRoles))
            {
                await _next(context);
                return;
            }

            Guid companyId;
            try
            {
                companyId = authService.GetTenantID();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to determine company id for user {UserId}", context.User.FindFirstValue(ClaimTypes.NameIdentifier));
                await DenyAsync(context, options, "Company non associata all'utenza.");
                return;
            }

            var subscription = await billingRepository.GetLatestSubscriptionForCompanyAsync(companyId, context.RequestAborted);
            if (subscription == null)
            {
                logger.LogWarning("Access blocked for company {CompanyId}: no subscription found.", companyId);
                await DenyAsync(context, options, "Nessuna subscription attiva trovata.");
                return;
            }

            if (!IsSubscriptionValid(subscription, options, logger))
            {
                logger.LogWarning("Access blocked for company {CompanyId}: subscription {SubscriptionId} in status {Status}.", companyId, subscription.SubscriptionID, subscription.RenewalStatus);
                await DenyAsync(context, options, "Subscription non valida o scaduta.");
                return;
            }

            await _next(context);
        }

        private static bool IsBypassedPath(PathString path, string[] bypassPaths)
        {
            if (bypassPaths == null || bypassPaths.Length == 0)
            {
                return false;
            }

            foreach (var entry in bypassPaths)
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                if (path.StartsWithSegments(entry, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUserInAdminRole(ClaimsPrincipal user, string[] adminRoles)
        {
            if (adminRoles == null || adminRoles.Length == 0)
            {
                return false;
            }

            foreach (var role in adminRoles)
            {
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                if (user.IsInRole(role))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSubscriptionValid(Company_Subscription subscription, SubscriptionAuthorizationOptions options, ILogger logger)
        {
            var now = DateTime.UtcNow;

            if (subscription.EndDate < now)
            {
                logger.LogInformation("Subscription {SubscriptionId} expired at {EndDate}.", subscription.SubscriptionID, subscription.EndDate);
                return false;
            }

            switch (subscription.RenewalStatus)
            {
                case RenewalStatus.ACTIVE:
                case RenewalStatus.TRIAL:
                    return true;
                case RenewalStatus.PAUSED:
                    if (options.GracePeriodDays <= 0)
                    {
                        logger.LogInformation("Subscription {SubscriptionId} paused without grace period.", subscription.SubscriptionID);
                        return false;
                    }

                    if (subscription.NextBillingDate == DateTime.MinValue)
                    {
                        logger.LogInformation("Subscription {SubscriptionId} paused but next billing date missing.", subscription.SubscriptionID);
                        return false;
                    }

                    var graceLimit = subscription.NextBillingDate.AddDays(options.GracePeriodDays);
                    if (graceLimit >= now)
                    {
                        return true;
                    }

                    logger.LogInformation("Subscription {SubscriptionId} exceeded grace period (limit {GraceLimit}).", subscription.SubscriptionID, graceLimit);
                    return false;
                case RenewalStatus.CANCELLED:
                case RenewalStatus.EXPIRED:
                case RenewalStatus.SUSPENDED:
                default:
                    return false;
            }
        }

        private static async Task DenyAsync(HttpContext context, SubscriptionAuthorizationOptions options, string message)
        {
            if (ShouldRedirect(context, options))
            {
                context.Response.Redirect(options.RedirectPath);
                return;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                var response = new ApiResponse<object?>(false, message, null, new List<string> { message });
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private static bool ShouldRedirect(HttpContext context, SubscriptionAuthorizationOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.RedirectPath))
            {
                return false;
            }

            if (context.Request.Path.Equals(options.RedirectPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (context.Request.Headers.TryGetValue("Accept", out var acceptValues))
            {
                if (acceptValues.Any(v => !string.IsNullOrWhiteSpace(v) && v.Contains("text/html", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
