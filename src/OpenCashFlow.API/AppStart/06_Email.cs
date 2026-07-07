using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Infrastructure.Email;

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
