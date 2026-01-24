// using Sentry;
// using Serilog;

// namespace OpenCashFlow.Api.AppStart;

// public static class SentryAppStart
// {
//     private const string Key = "AppStart:SentryEnabled";

//     public static bool IsSentryEnabled(this WebApplication app)
//         => app.Properties.TryGetValue(Key, out var v) && v is bool b && b;

//     public static WebApplicationBuilder AppStartConfigureSentry(this WebApplicationBuilder builder)
//     {
//         var sentryDsn = builder.Configuration["SENTRY_DSN"]
//             ?? builder.Configuration["Sentry:Dsn"]
//             ?? Environment.GetEnvironmentVariable("SENTRY_DSN");

//         bool enabled = false;

//         if (!string.IsNullOrWhiteSpace(sentryDsn))
//         {
//             try
//             {
//                 _ = new Dsn(sentryDsn); // validate
//                 enabled = true;

//                 builder.WebHost.UseSentry(options =>
//                 {
//                     options.Dsn = sentryDsn;
//                 });
//             }
//             catch (Exception ex)
//             {
//                 Log.Warning(ex, "Sentry DSN non valido: Sentry disabilitato per questo avvio.");
//             }
//         }
//         else
//         {
//             Log.Information("Sentry DSN mancante: Sentry disabilitato per questo avvio.");
//         }

//         builder.Properties[Key] = enabled;
//         return builder;
//     }

//     public static WebApplication AppStartPropagateSentryFlag(this WebApplication app, WebApplicationBuilder builder)
//     {
//         // Copia il flag da builder -> app
//         if (builder.Properties.TryGetValue(Key, out var v) && v is bool b)
//             app.Properties[Key] = b;
//         else
//             app.Properties[Key] = false;

//         return app;
//     }
// }