using OpenCashFlow.Admin.Models.Billing;
using OpenCashFlow.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using global::Shared.DTOs.Billing;
using global::Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OpenCashFlow.Admin.Controllers
{
    [Authorize(Policy = "GIManagers")]
    public class BillingController(BillingPlansAPIService billingPlansService, ILogger<BillingController> logger) : Controller
    {
        private const string SavedViewsCookieName = "gi-admin-billing-views";
        private static readonly JsonSerializerOptions SavedViewSerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly BillingPlansAPIService _billingPlansService = billingPlansService;
        private readonly ILogger<BillingController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Subscriptions([FromQuery] BillingSubscriptionFilters? filters, CancellationToken cancellationToken)
        {
            var sanitizedFilters = SanitizeFilters(filters);

            var dtoFilters = new BillingSubscription_Filter_DTO
            {
                Search = sanitizedFilters.Search,
                RenewalStatus = sanitizedFilters.Status,
                RenewalStatuses = sanitizedFilters.Statuses,
                TenantID = sanitizedFilters.TenantID,
                BillingCycle = sanitizedFilters.BillingCycle,
                HasTrial = sanitizedFilters.HasTrial,
                ActiveOn = sanitizedFilters.ActiveOn,
                StartFrom = sanitizedFilters.StartFrom,
                StartTo = sanitizedFilters.StartTo,
                EndFrom = sanitizedFilters.EndFrom,
                EndTo = sanitizedFilters.EndTo,
                NextBillingFrom = sanitizedFilters.NextBillingFrom,
                NextBillingTo = sanitizedFilters.NextBillingTo,
                IncludeAllCompanies = sanitizedFilters.IncludeAllCompanies,
                Page = sanitizedFilters.Page,
                PageSize = sanitizedFilters.PageSize
            };

            var response = await _billingPlansService.GetSubscriptionsAsync(dtoFilters, cancellationToken);
            var items = response.Data?.ToList() ?? [];

            var savedViews = GetSavedViews();

            var viewModel = new BillingSubscriptionsListViewModel
            {
                Items = items,
                Filters = sanitizedFilters,
                HasNextPage = items.Count >= sanitizedFilters.PageSize,
                ErrorMessage = response.Success ? null : response.Message,
                SavedViews = savedViews
            };

            if (!response.Success)
            {
                _logger.LogWarning("Errore durante il recupero delle subscription: {Message}", response.Message);
            }

            ViewData["Title"] = "Piani & Subscription";
            return View("Subscriptions", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveView([FromForm] string name, [FromForm] BillingSubscriptionFilters filters)
        {
            var sanitizedFilters = SanitizeFilters(filters);

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["BillingSaveError"] = "Il nome della vista è obbligatorio.";
                return RedirectToFilters(sanitizedFilters);
            }

            var safeName = name.Trim();
            var queryString = BuildQueryString(sanitizedFilters);
            var savedViews = GetSavedViews().ToList();
            var existingIndex = savedViews.FindIndex(v => string.Equals(v.Name, safeName, StringComparison.OrdinalIgnoreCase));
            var newEntry = new BillingSavedView(safeName, queryString, DateTime.UtcNow);

            if (existingIndex >= 0)
            {
                savedViews[existingIndex] = newEntry;
            }
            else
            {
                savedViews.Insert(0, newEntry);
                const int maxViews = 10;
                if (savedViews.Count > maxViews)
                {
                    savedViews.RemoveRange(maxViews, savedViews.Count - maxViews);
                }
            }

            PersistSavedViews(savedViews);
            TempData["BillingSaveMessage"] = "Vista salvata correttamente.";
            return Redirect($"{Url.Action(nameof(Subscriptions))}{queryString}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteView([FromForm] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Subscriptions));
            }

            var savedViews = GetSavedViews().ToList();
            var removed = savedViews.RemoveAll(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                PersistSavedViews(savedViews);
                TempData["BillingSaveMessage"] = "Vista rimossa.";
            }

            return RedirectToAction(nameof(Subscriptions));
        }

        [HttpGet]
        public IActionResult LoadView(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Subscriptions));
            }

            var target = GetSavedViews()
                .FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

            if (target == null)
            {
                TempData["BillingSaveError"] = "Vista non trovata.";
                return RedirectToAction(nameof(Subscriptions));
            }

            return Redirect($"{Url.Action(nameof(Subscriptions))}{target.QueryString}");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.GetDashboardKPIAsync(cancellationToken);

            var viewModel = new BillingDashboardViewModel
            {
                KPI = response.Data,
                ErrorMessage = response.Success ? null : response.Message
            };

            if (!response.Success)
            {
                _logger.LogWarning("Errore durante il recupero dei KPI dashboard: {Message}", response.Message);
            }

            ViewData["Title"] = "Dashboard Billing";
            return View("Dashboard", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManageSubscription(Guid id, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.GetSubscriptionDetailAsync(id, cancellationToken);

            // Carica anche la lista dei piani disponibili per il cambio piano
            var plansResponse = await _billingPlansService.GetPlansAsync(new BillingPlan_Filter_DTO { OnlyActive = true }, cancellationToken);

            // Carica audit log
            var auditLogResponse = await _billingPlansService.GetSubscriptionAuditLogAsync(id, cancellationToken);

            var viewModel = new ManageSubscriptionViewModel
            {
                Subscription = response.Data,
                AvailablePlans = plansResponse.Data ?? [],
                AuditLog = auditLogResponse.Data ?? [],
                ErrorMessage = response.Success ? null : response.Message,
                SuccessMessage = TempData["SuccessMessage"] as string
            };

            if (!response.Success)
            {
                _logger.LogWarning("Errore durante il recupero dei dettagli subscription {SubscriptionId}: {Message}", id, response.Message);
            }

            ViewData["Title"] = $"Gestione Subscription - {viewModel.Subscription?.CompanyName ?? "N/A"}";
            return View("ManageSubscription", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncSubscription(Guid id, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.SyncSubscriptionAsync(id, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendTrial(Guid id, int days, string? reason, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.ExtendTrialAsync(id, days, reason, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCredit(Guid id, decimal amount, string description, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.ApplyCreditAsync(id, amount, description, "EUR", cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvoice(Guid id, string stripeInvoiceId, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.SendInvoiceAsync(stripeInvoiceId, null, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCancelSubscription(Guid id, bool immediately, string adminReason, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.AdminCancelSubscriptionAsync(id, immediately, adminReason, true, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualExtendSubscription(Guid id, int months, string? reason, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.ManualExtendSubscriptionAsync(id, months, reason, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePlan(Guid id, Guid newPlanId, string? reason, bool prorate, CancellationToken cancellationToken)
        {
            var response = await _billingPlansService.AdminChangePlanAsync(id, newPlanId, reason, prorate, cancellationToken);

            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(ManageSubscription), new { id });
        }

        private static BillingSubscriptionFilters SanitizeFilters(BillingSubscriptionFilters? filters)
        {
            filters ??= new BillingSubscriptionFilters();

            filters.Page = Math.Max(1, filters.Page);
            filters.PageSize = Math.Clamp(filters.PageSize, 1, 200);
            filters.Search = string.IsNullOrWhiteSpace(filters.Search) ? null : filters.Search.Trim();

            filters.Statuses = filters.Statuses?
                .Where(status => Enum.IsDefined(typeof(RenewalStatus), status))
                .Distinct()
                .ToList();

            if (filters.Status.HasValue)
            {
                filters.Statuses ??= [];
                if (!filters.Statuses.Contains(filters.Status.Value))
                {
                    filters.Statuses.Add(filters.Status.Value);
                }
            }

            filters.ActiveOn = NormalizeDate(filters.ActiveOn);
            filters.StartFrom = NormalizeDate(filters.StartFrom);
            filters.StartTo = NormalizeDate(filters.StartTo);
            filters.EndFrom = NormalizeDate(filters.EndFrom);
            filters.EndTo = NormalizeDate(filters.EndTo);
            filters.NextBillingFrom = NormalizeDate(filters.NextBillingFrom);
            filters.NextBillingTo = NormalizeDate(filters.NextBillingTo);

            return filters;
        }

        private static DateTime? NormalizeDate(DateTime? input)
        {
            if (!input.HasValue)
            {
                return null;
            }

            var value = input.Value;
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private IReadOnlyList<BillingSavedView> GetSavedViews()
        {
            if (!Request.Cookies.TryGetValue(SavedViewsCookieName, out var cookieValue) ||
                string.IsNullOrWhiteSpace(cookieValue))
            {
                return Array.Empty<BillingSavedView>();
            }

            try
            {
                var views = JsonSerializer.Deserialize<List<BillingSavedView>>(cookieValue, SavedViewSerializerOptions);
                return views?
                           .OrderByDescending(v => v.SavedAtUtc)
                           .ToList()
                       ?? new List<BillingSavedView>();
            }
            catch
            {
                return Array.Empty<BillingSavedView>();
            }
        }

        private void PersistSavedViews(IEnumerable<BillingSavedView> views)
        {
            var serialized = JsonSerializer.Serialize(views, SavedViewSerializerOptions);
            Response.Cookies.Append(
                SavedViewsCookieName,
                serialized,
                new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps,
                    Expires = DateTimeOffset.UtcNow.AddMonths(3)
                });
        }

        private static string BuildQueryString(BillingSubscriptionFilters filters)
        {
            var pairs = BuildQueryPairs(filters);
            var uri = QueryHelpers.AddQueryString(string.Empty, pairs);
            return uri.StartsWith('?') ? uri : $"?{uri}";
        }

        private IActionResult RedirectToFilters(BillingSubscriptionFilters filters)
        {
            var qs = BuildQueryString(filters);
            return Redirect($"{Url.Action(nameof(Subscriptions))}{qs}");
        }

        private static IEnumerable<KeyValuePair<string, string?>> BuildQueryPairs(BillingSubscriptionFilters filters)
        {
            var pairs = new List<KeyValuePair<string, string?>>();

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                pairs.Add(new("Search", filters.Search));
            }

            if (filters.Status.HasValue)
            {
                pairs.Add(new("Status", filters.Status.Value.ToString()));
            }

            if (filters.Statuses != null)
            {
                foreach (var status in filters.Statuses.Where(s => Enum.IsDefined(typeof(RenewalStatus), s)))
                {
                    pairs.Add(new("Statuses", status.ToString()));
                }
            }

            if (filters.TenantID.HasValue)
            {
                pairs.Add(new("TenantID", filters.TenantID.Value.ToString()));
            }

            if (filters.BillingCycle.HasValue)
            {
                pairs.Add(new("BillingCycle", filters.BillingCycle.Value.ToString()));
            }

            if (filters.HasTrial.HasValue)
            {
                pairs.Add(new("HasTrial", filters.HasTrial.Value.ToString().ToLowerInvariant()));
            }

            if (filters.ActiveOn.HasValue)
            {
                pairs.Add(new("ActiveOn", filters.ActiveOn.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.StartFrom.HasValue)
            {
                pairs.Add(new("StartFrom", filters.StartFrom.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.StartTo.HasValue)
            {
                pairs.Add(new("StartTo", filters.StartTo.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.EndFrom.HasValue)
            {
                pairs.Add(new("EndFrom", filters.EndFrom.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.EndTo.HasValue)
            {
                pairs.Add(new("EndTo", filters.EndTo.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.NextBillingFrom.HasValue)
            {
                pairs.Add(new("NextBillingFrom", filters.NextBillingFrom.Value.ToString("yyyy-MM-dd")));
            }

            if (filters.NextBillingTo.HasValue)
            {
                pairs.Add(new("NextBillingTo", filters.NextBillingTo.Value.ToString("yyyy-MM-dd")));
            }

            pairs.Add(new("IncludeAllCompanies", filters.IncludeAllCompanies ? "true" : "false"));
            pairs.Add(new("Page", filters.Page.ToString()));
            pairs.Add(new("PageSize", filters.PageSize.ToString()));

            return pairs;
        }
    }
}
