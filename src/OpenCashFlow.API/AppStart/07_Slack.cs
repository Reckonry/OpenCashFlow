using Microsoft.Extensions.Options;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Infrastructure.Notifications;

namespace OpenCashFlow.Api.AppStart;

public static class SlackAppStart
{
    public static WebApplicationBuilder AppStartConfigureSlack(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SlackOptions>(builder.Configuration.GetSection("Slack"));

        builder.Services.AddHttpClient<ISlackNotifier, SlackNotifier>("SlackClient", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<SlackOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });

        return builder;
    }
}
