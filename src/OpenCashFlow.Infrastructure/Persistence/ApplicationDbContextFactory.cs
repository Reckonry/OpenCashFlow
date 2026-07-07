using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace OpenCashFlow.Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var repoRoot = Directory.GetCurrentDirectory();
            var apiConfigPath = Path.GetFullPath(Path.Combine(repoRoot, "..", "OpenCashFlow.API"));

            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING")
                ?? TryReadConnectionString(Directory.Exists(apiConfigPath) ? apiConfigPath : repoRoot)
                ?? "Host=localhost;Database=opencashflow;Username=postgres;Password=postgres;";

            optionsBuilder.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static string? TryReadConnectionString(string basePath)
        {
            foreach (var fileName in new[] { "appsettings.Development.json", "appsettings.json" })
            {
                var path = Path.Combine(basePath, fileName);
                if (!File.Exists(path))
                {
                    continue;
                }

                using var stream = File.OpenRead(path);
                using var document = JsonDocument.Parse(stream);
                if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                    && connectionStrings.TryGetProperty("DefaultConnectionString", out var value))
                {
                    return value.GetString();
                }
            }

            return null;
        }
    }
}
