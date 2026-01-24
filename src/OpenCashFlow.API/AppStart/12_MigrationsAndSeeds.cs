using Microsoft.EntityFrameworkCore;
using Serilog;
using global::Shared.Data;

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
                var pending = db.Database.GetPendingMigrations();
                if (pending.Any())
                    db.Database.Migrate();

                if (db.Database.GetPendingMigrations().Any())
                {
                    Log.Information("Applying {Count} pending migrations...", db.Database.GetPendingMigrations().Count());
                    db.Database.Migrate();
                    Log.Information("Database migrations applied successfully.");
                }

                var companies = await db.Company_DS.AsNoTracking().Select(c => c.TenantID).ToListAsync();
                var existing = await db.CashBalances.AsNoTracking().Select(b => b.CompanyId).ToListAsync();
                var missing = companies.Except(existing).ToList();

                if (missing.Count > 0)
                {
                    foreach (var cid in missing)
                    {
                        db.CashBalances.Add(new global::Shared.Models.Cash.CashBalance
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