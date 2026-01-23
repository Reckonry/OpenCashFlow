// using Microsoft.Extensions.Options;
// using System.Reflection;

// namespace OpenCashFlow.Api.AppStart;

// public static class StripeAppStart
// {
//     public static WebApplicationBuilder AppStartConfigureStripe(this WebApplicationBuilder builder)
//     {
//         builder.Services.AddOptions<StripeSettings>()
//             .Bind(builder.Configuration.GetSection("Stripe"));

//         builder.Services.AddSingleton<Stripe.IStripeClient>(sp =>
//         {
//             var options = sp.GetRequiredService<IOptions<StripeSettings>>().Value;
//             var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("StripeStartup");

//             bool IsMissing(string? value) =>
//                 string.IsNullOrWhiteSpace(value) ||
//                 value.Contains("placeholder", StringComparison.OrdinalIgnoreCase);

//             var missingFields = new List<string>();
//             if (IsMissing(options.SecretKey)) missingFields.Add(nameof(options.SecretKey));
//             if (IsMissing(options.PublishableKey)) missingFields.Add(nameof(options.PublishableKey));
//             if (IsMissing(options.WebhookSecret)) missingFields.Add(nameof(options.WebhookSecret));

//             if (missingFields.Count > 0)
//             {
//                 logger.LogWarning("Stripe configuration incompleta: mancano {MissingFields}.", string.Join(", ", missingFields));
//             }
//             else
//             {
//                 Stripe.StripeConfiguration.ApiKey = options.SecretKey;
//                 Stripe.StripeConfiguration.AppInfo = new Stripe.AppInfo
//                 {
//                     Name = "OpenCashFlow",
//                     Url = "https://opencashflow.cloud",
//                     Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
//                 };
//                 Stripe.StripeConfiguration.MaxNetworkRetries = 2;
//             }

//             return new Stripe.StripeClient(new Stripe.StripeClientOptions
//             {
//                 ApiKey = options.SecretKey
//             });
//         });

//         return builder;
//     }
// }