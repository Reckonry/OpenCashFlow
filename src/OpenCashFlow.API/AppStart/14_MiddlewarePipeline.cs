using Serilog;
using OpenCashFlow.API.Middleware;

namespace OpenCashFlow.Api.AppStart;

public static class MiddlewarePipelineAppStart
{
    public static WebApplication AppStartConfigureMiddlewarePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors(CorsAppStart.PolicyName);

        // Rate limiter (se vuoi applicarlo globalmente)
        app.AppStartUseRateLimiting();

        // Sentry tracing solo se abilitato
        // if (app.IsSentryEnabled())
        // {
        //     app.UseSentryTracing();
        // }

        app.UseAuthentication();
        app.UseMiddleware<SubscriptionAuthorizationMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static WebApplication AppStartValidateStripeInProduction(this WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            try
            {
                using var scope = app.Services.CreateScope();
                _ = scope.ServiceProvider.GetService<Stripe.IStripeClient>();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Stripe client non inizializzato (API key mancante/invalid). Avvio prosegue.");
            }
        }

        return app;
    }
}