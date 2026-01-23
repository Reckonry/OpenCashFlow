using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using global::Shared.Data;

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
                .UseNpgsql(pgConnString)
                .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
        );

        return builder;
    }
}