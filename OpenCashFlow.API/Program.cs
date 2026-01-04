using OpenCashFlow.API.Repositories;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Shared.Mappings;
using OpenCashFlow.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using global::Shared.Data;
using global::Shared.Options;
using global::Shared.Services;
using global::Shared.Services.Interfaces;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

#region Logging

string solutionLogs = Path.Combine(Directory.GetCurrentDirectory(), "..", "Logs");
Directory.CreateDirectory(solutionLogs);

// Configura Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File(
        path: Path.Combine(solutionLogs, "log-APP-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.WebHost.UseSentry();
builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Warning); // ⬅️ metti Warning o Error
builder.Logging.AddConsole();

// 🔇 Filtri per silenziare EF, System, Hosting
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

#endregion

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables().Build();
var pgConnString = Environment.GetEnvironmentVariable("DEFAULT_CONN_STRING") ??
    configuration.GetConnectionString("DefaultConnectionString");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(pgConnString)
        // If EF detects model changes at runtime, log instead of throwing
        .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
);

#region RateLimiting
builder.Services.AddRateLimiter(options =>
{
    // Regola globale: Limita tutte le richieste a 5 al secondo
    options.AddFixedWindowLimiter("global", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(1); // Finestra di 1 secondo
        limiterOptions.PermitLimit = 5;                 // Limite di 5 richieste per finestra
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Ordine della coda
        limiterOptions.QueueLimit = 2;                 // Consenti 2 richieste in coda
    });

    // Regola per endpoint specifico
    options.AddFixedWindowLimiter("customers-limiter", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1); // Finestra di 1 minuto
        limiterOptions.PermitLimit = 20;                // Limite di 20 richieste per finestra
    });
});
#endregion
#region Bearer Auth
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    // Configure validation parameters
    var audiences = builder.Configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        // Support single or multiple audiences from configuration
        ValidAudience = audiences == null || audiences.Length == 0 ? builder.Configuration["JwtSettings:Audience"] : null,
        ValidAudiences = audiences != null && audiences.Length > 0 ? audiences : null
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to get token from cookie if not in Authorization header
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies[global::Shared.Core.Configuration.AuthCookieName];
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("GIManagers", policy => policy.RequireClaim(ClaimTypes.Role, "GIManagers"));
#endregion
#region Email Support
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailOption>() ?? throw new InvalidOperationException("EmailConfiguration mancante!");

builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();
#endregion
#region Slack connection
builder.Services.Configure<SlackOptions>(builder.Configuration.GetSection("Slack"));
builder.Services.AddHttpClient<ISlackNotifier, SlackNotifier>("SlackClient", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<SlackOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
});
#endregion
#region Stripe
builder.Services.AddOptions<StripeSettings>()
    .Bind(builder.Configuration.GetSection("Stripe"));

builder.Services.AddSingleton<Stripe.IStripeClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<StripeSettings>>().Value;
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("StripeStartup");

    bool IsMissing(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Contains("placeholder", StringComparison.OrdinalIgnoreCase);

    var missingFields = new List<string>();
    if (IsMissing(options.SecretKey))
    {
        missingFields.Add(nameof(options.SecretKey));
    }
    if (IsMissing(options.PublishableKey))
    {
        missingFields.Add(nameof(options.PublishableKey));
    }
    if (IsMissing(options.WebhookSecret))
    {
        missingFields.Add(nameof(options.WebhookSecret));
    }

    if (missingFields.Count > 0)
    {
        logger.LogWarning("Stripe configuration incompleta: mancano {MissingFields}.", string.Join(", ", missingFields));
    }
    else
    {
        Stripe.StripeConfiguration.ApiKey = options.SecretKey;
        Stripe.StripeConfiguration.AppInfo = new Stripe.AppInfo
        {
            Name = "OpenCashFlow",
            Url = "https://opencashflow.cloud",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        };
        Stripe.StripeConfiguration.MaxNetworkRetries = 2;
    }

    return new Stripe.StripeClient(new Stripe.StripeClientOptions
    {
        ApiKey = options.SecretKey
    });
});
#endregion

#region Subscription Authorization
builder.Services.AddOptions<SubscriptionAuthorizationOptions>()
    .Bind(builder.Configuration.GetSection("SubscriptionAuthorization"));

builder.Services.PostConfigure<SubscriptionAuthorizationOptions>(opts =>
{
    if (opts.BypassPaths == null || opts.BypassPaths.Length == 0)
    {
        opts.BypassPaths = new[]
        {
            "/v1/Billing",
            "/swagger",
            "/api/stripe/webhook",
            "/health",
            "/subscription-expired"
        };
    }

    if (opts.AdminRoles == null || opts.AdminRoles.Length == 0)
    {
        opts.AdminRoles = new[] { "GIManagers" };
    }
});
#endregion
// Add services to the container.

builder.Services.AddControllers();
// Swagger/OpenAPI (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OpenCashFlow API",
        Version = "v1",
        Description = "API per gestione flussi di cassa"
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#if DEBUG
try
{
    builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(MappingProfile).Assembly);
}
catch (ReflectionTypeLoadException ex)
{
    var details = string.Join("\n---\n",
        ex.LoaderExceptions.Select(le => le?.Message + (le is FileNotFoundException f ? $" | Missing: {f.FileName}" : "")));
    Console.WriteLine(details);
    throw;
}
#endif
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#region Custom Scopes
#region Repositories
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();
#endregion

#region Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICashService, CashService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IStripeSyncService, StripeSyncService>();
builder.Services.AddScoped<IStripeWebhookEventService, StripeWebhookEventService>();
builder.Services.AddScoped<IStripeWebhookProcessor, StripeWebhookProcessor>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
#endregion
//builder.Services.AddMemoryCache();
//builder.Services.AddScoped<IFeatureService, OpenCashFlow.API.Services.FeatureService>();
builder.Services.AddHttpContextAccessor();
#endregion


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // Invia le versioni supportate nell'header della risposta
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAPIService", policyBuilder =>
    {
        // Read allowed origins from configuration (fallback to AppUrl)
        var originsFromConfig = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var origins = originsFromConfig.Length > 0
            ? originsFromConfig
            : new[] { builder.Configuration["AppUrl"] ?? string.Empty };

        // Normalize by trimming trailing slashes to match origin format
        origins = origins
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.TrimEnd('/'))
            .ToArray();

        policyBuilder
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-SignalR-User-Agent")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));

        // Allow credentials only if explicitly configured
        if (builder.Configuration.GetValue<bool>("Cors:AllowCredentials"))
        {
            policyBuilder.AllowCredentials();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    try
    {
        using var scope = app.Services.CreateScope();
        // Prova a risolvere il client (fallisce se la key è mancante)
        _ = scope.ServiceProvider.GetService<Stripe.IStripeClient>();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Stripe client non inizializzato (API key mancante/invalid). Avvio prosegue.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenCashFlow API v1");
        c.RoutePrefix = "swagger";
    });
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (db.Database.IsRelational())
        {
            // ok con SqlServer/Postgres/SQLite file
            var pending = db.Database.GetPendingMigrations();
            if (pending.Any())
                db.Database.Migrate();

            // Apply any pending migrations. Safe to call if up-to-date.
            if (db.Database.GetPendingMigrations().Any())
            {
                Log.Information("Applying {Count} pending migrations...", db.Database.GetPendingMigrations().Count());
                db.Database.Migrate();
                Log.Information("Database migrations applied successfully.");
            }

            // Ensure a CashBalance row exists for all companies
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
    catch (System.Exception ex)
    {
        Log.Error(ex, "Failed to apply database migrations at startup");
        throw; // Fail fast so we don't run against a mismatched schema
    }
}


app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error != null)
        {
            var exception = exceptionFeature.Error;
            int statusCode = 500;
            string message = exception.Message;

            // Handle specific exception types
            if (exception is System.ArgumentException or System.ArgumentNullException)
            {
                statusCode = 400; // Bad Request
            }
            else if (exception is System.UnauthorizedAccessException)
            {
                statusCode = 403; // Forbidden
            }
            else if (exception is System.Collections.Generic.KeyNotFoundException)
            {
                statusCode = 404; // Not Found
            }

            var error = new
            {
                Message = message,
                StackTrace = app.Environment.IsDevelopment() ? exception.StackTrace : null, // Only show stack trace in dev
                Path = exceptionFeature.Path,
                StatusCode = statusCode
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(error);
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAPIService");

app.UseSentryTracing();
// Ensure authentication runs before authorization so [Authorize] works
app.UseAuthentication();
app.UseMiddleware<SubscriptionAuthorizationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { } // NON RIMUOVERE, serve per effettuare test in CI/CD con db in memory sull'API
