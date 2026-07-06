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

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
