using OpenCashFlow.Test.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenCashFlow.Infrastructure.Persistence;
using System.Text;

namespace OpenCashFlow.Test.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly bool _useFakeAuth;
        private readonly string _dbIdentifier;
        private readonly bool _usePostgres;

        public CustomWebApplicationFactory(string dbIdentifier, bool useFakeAuth = true)
        {
            _dbIdentifier = dbIdentifier;
            _useFakeAuth = useFakeAuth;
            // If dbIdentifier contains connection string keywords, use PostgreSQL
            _usePostgres = dbIdentifier.Contains("Host=") || dbIdentifier.Contains("Server=");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:AuthPermitLimit"] = "1000",
                    ["RateLimiting:AuthWindowSeconds"] = "1"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("DbContextOptions") == true)
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Register database based on configuration
                if (_usePostgres)
                {
                    // Use PostgreSQL with the provided connection string
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_dbIdentifier);
                    });
                }
                else
                {
                    // Use in-memory database for faster tests (transactions not supported)
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_dbIdentifier)
                            .ConfigureWarnings(warnings =>
                                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                    });
                }

                // Configure authentication

                var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                var key = Encoding.ASCII.GetBytes(config["JwtSettings:SecretKey"]!);

                if (!services.Any(s => s.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>)))
                {
                    services.AddAuthentication("Bearer")
                        .AddJwtBearer("Bearer", options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = false,
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero,
                                ValidIssuer = config["JwtSettings:Issuer"],
                                IssuerSigningKey = new SymmetricSecurityKey(key)
                            };
                        });
                }


                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                if (_usePostgres)
                {
                    // Use migrations for PostgreSQL to ensure proper schema
                    db.Database.Migrate();
                }
                else
                {
                    // Use EnsureCreated for in-memory database
                    db.Database.EnsureCreated();
                }

                TestHelpers.SeedAllTestData(db);
            });
        }

        public async Task<string> GenerateJwtTokenAsync(Guid userId)
        {
            return await JwtTokenGenerator.GenerateTokenAsync(Services, userId, role: "CompanyAdmin");
        }

        public ApplicationDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }
    }
}
