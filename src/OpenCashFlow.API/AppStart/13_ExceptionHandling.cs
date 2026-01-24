using Microsoft.AspNetCore.Diagnostics;

namespace OpenCashFlow.Api.AppStart;

public static class ExceptionHandlingAppStart
{
    public static WebApplication AppStartConfigureExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionFeature?.Error != null)
                {
                    var exception = exceptionFeature.Error;
                    int statusCode = 500;
                    string message = exception.Message;

                    if (exception is ArgumentException or ArgumentNullException) statusCode = 400;
                    else if (exception is UnauthorizedAccessException) statusCode = 403;
                    else if (exception is KeyNotFoundException) statusCode = 404;

                    var error = new
                    {
                        Message = message,
                        StackTrace = app.Environment.IsDevelopment() ? exception.StackTrace : null,
                        Path = exceptionFeature.Path,
                        StatusCode = statusCode
                    };

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsJsonAsync(error);
                }
            });
        });

        return app;
    }
}