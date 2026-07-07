using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Api.AppStart;

public static class MigrationsAndSeedsAppStart
{
    public static async Task<WebApplication> AppStartApplyMigrationsAndSeeds(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            if (db.Database.IsRelational())
            {
                var autoMigrate = string.Equals(
                    app.Configuration["AUTO_MIGRATE"],
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                var pending = db.Database.GetPendingMigrations().ToList();
                if (autoMigrate)
                {
                    if (pending.Count > 0)
                    {
                        Log.Information("Applying {Count} pending migrations...", pending.Count);
                        db.Database.Migrate();
                        Log.Information("Database migrations applied successfully.");
                    }
                }
                else
                {
                    Log.Information("AUTO_MIGRATE is not enabled. Skipping startup database migrations.");
                    if (pending.Count > 0)
                    {
                        return app;
                    }
                }

                if (!await db.Database.CanConnectAsync())
                {
                    return app;
                }

                var companies = await db.Company_DS.AsNoTracking().Select(c => c.TenantID).ToListAsync();
                var existing = await db.CashBalances.AsNoTracking().Select(b => b.CompanyId).ToListAsync();
                var missing = companies.Except(existing).ToList();

                if (missing.Count > 0)
                {
                    foreach (var cid in missing)
                    {
                        db.CashBalances.Add(new OpenCashFlow.Infrastructure.Persistence.Entities.Cash.CashBalance
                        {
                            CompanyId = cid,
                            Balance = 0m,
                            LastUpdatedUtc = DateTimeOffset.UtcNow
                        });
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply database migrations at startup");
            throw;
        }

        return app;
    }
}
