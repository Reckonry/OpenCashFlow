using Microsoft.OpenApi.Models;

namespace OpenCashFlow.Api.AppStart;

public static class SwaggerAppStart
{
    public static WebApplicationBuilder AppStartConfigureSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OpenCashFlow API",
                Version = "v1",
                Description = "API per gestione flussi di cassa"
            });
        });

        builder.Services.AddOpenApi();
        return builder;
    }

    public static WebApplication AppStartUseSwaggerIfDev(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenCashFlow API v1");
                c.RoutePrefix = "swagger";
            });

            app.MapOpenApi();
        }

        return app;
    }
}