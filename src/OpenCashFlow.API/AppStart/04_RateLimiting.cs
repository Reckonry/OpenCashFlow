using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace OpenCashFlow.Api.AppStart;

public static class RateLimitingAppStart
{
    public static WebApplicationBuilder AppStartConfigureRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("global", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(1);
                limiterOptions.PermitLimit = 5;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2;
            });

            options.AddFixedWindowLimiter("customers-limiter", limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 20;
            });

            options.AddFixedWindowLimiter("auth-limiter", limiterOptions =>
            {
                var authWindowSeconds = builder.Configuration.GetValue<int?>("RateLimiting:AuthWindowSeconds") ?? 60;
                var authPermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:AuthPermitLimit") ?? 5;

                limiterOptions.Window = TimeSpan.FromSeconds(authWindowSeconds);
                limiterOptions.PermitLimit = authPermitLimit;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        return builder;
    }

    public static WebApplication AppStartUseRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
