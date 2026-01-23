using global::Shared.Options;

namespace OpenCashFlow.Api.AppStart;

public static class SubscriptionAuthorizationAppStart
{
    public static WebApplicationBuilder AppStartConfigureSubscriptionAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<SubscriptionAuthorizationOptions>()
            .Bind(builder.Configuration.GetSection("SubscriptionAuthorization"));

        builder.Services.PostConfigure<SubscriptionAuthorizationOptions>(opts =>
        {
            if (opts.BypassPaths == null || opts.BypassPaths.Length == 0)
            {
                opts.BypassPaths = new[]
                {
                    "/v1/Billing",
                    "/swagger",
                    "/api/stripe/webhook",
                    "/health",
                    "/subscription-expired"
                };
            }

            if (opts.AdminRoles == null || opts.AdminRoles.Length == 0)
            {
                opts.AdminRoles = new[] { "GIManagers" };
            }
        });

        return builder;
    }
}