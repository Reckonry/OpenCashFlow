using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Api.AppStart;

public static class DatabaseAppStart
{
    public static WebApplicationBuilder AppStartConfigureDatabase(this WebApplicationBuilder builder)
    {
        var pgConnString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING")
            ?? builder.Configuration.GetConnectionString("DefaultConnectionString");

        if (string.IsNullOrWhiteSpace(pgConnString))
            throw new InvalidOperationException("Connection string mancante (DEFAULT_CONN_STRING o ConnectionStrings:DefaultConnectionString).");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    pgConnString,
                    npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
        );

        return builder;
    }
}
