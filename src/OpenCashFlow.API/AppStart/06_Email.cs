using global::Shared.Options;
using global::Shared.Services.Interfaces;
using global::Shared.Services;

namespace OpenCashFlow.Api.AppStart;

public static class EmailAppStart
{
    public static WebApplicationBuilder AppStartConfigureEmail(this WebApplicationBuilder builder)
    {
        var emailConfig = builder.Configuration
            .GetSection("EmailConfiguration")
            .Get<EmailOption>() ?? throw new InvalidOperationException("EmailConfiguration mancante!");

        builder.Services.AddSingleton(emailConfig);
        builder.Services.AddScoped<IEmailSender, EmailSender>();

        return builder;
    }
}