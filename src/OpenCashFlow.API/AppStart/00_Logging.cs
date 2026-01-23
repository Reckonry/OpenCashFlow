using Serilog;

namespace OpenCashFlow.Api.AppStart;

public static class LoggingAppStart
{
    public static WebApplicationBuilder AppStartConfigureLogging(this WebApplicationBuilder builder)
    {
        // Cartella Logs a livello solution: ../Logs rispetto a src/OpenCashFlow.Api
        string solutionLogs = Path.Combine(Directory.GetCurrentDirectory(), "..", "Logs");
        Directory.CreateDirectory(solutionLogs);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File(
                path: Path.Combine(solutionLogs, "log-APP-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        builder.Host.UseSerilog();

        // Default logging providers (console, ecc.)
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddConsole();

        // Filters to silence EF, System, Hosting
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

        return builder;
    }
}