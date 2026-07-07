using OpenCashFlow.WebApp.Hubs;
using OpenCashFlow.WebApp.Services;
using OpenCashFlow.WebApp.Services.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using System.Text;

#region Logging
var solutionLogs = Path.Combine(Directory.GetCurrentDirectory(), "..", "Logs");

Directory.CreateDirectory(solutionLogs);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File(
        // file pattern: log-APP-2025-05-25.txt, log-APP-2025-05-26.txt
        path: Path.Combine(solutionLogs, "log-APP-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    //.WriteTo.Slack(
    //    webhookUrl: "https://hooks.slack.com/services/<webhook-url>",
    //    restrictedToMinimumLevel: LogEventLevel.Error,
    //    batchSizeLimit: 1,
    //    period: TimeSpan.FromSeconds(1),
    //    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}"
    //)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
// Load configuration based on environment (Development, Staging, Production)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // support variables from systemd or shell
//builder.WebHost.UseSentry();

builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
#endregion


// HttpClient configuration for API access
builder.Services.AddHttpClient("API-Client", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Account:API"]!); // OpenCashFlow.API URL
}).AddHttpMessageHandler<BearerTokenHandler>();

// Add services to the container.

builder.Services.AddScoped<AuthenticationAPIService>();
builder.Services.AddScoped<SetupAPIService>();
builder.Services.AddScoped<CompanyAPIService>();
builder.Services.AddScoped<EmployeeAPIService>();
builder.Services.AddScoped<PaymentAPIService>();
builder.Services.AddScoped<RoleAPIService>();

builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddSignalR();

#region Bearer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // HTTPS enforcement: enabled outside Development
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;

        var audiences = builder.Configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
        var keyBytes = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidAudience = audiences == null || audiences.Length == 0 ? builder.Configuration["JwtSettings:Audience"] : null,
            ValidAudiences = audiences != null && audiences.Length > 0 ? audiences : null
        };

        // Read the token from the "authCookie" cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Look for the token in the "authCookie" cookie
                var token = context.Request.Cookies[OpenCashFlow.Contracts.Core.Configuration.AuthCookieName];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token; // Pass the token to the JWT middleware
                }
                else
                {
                    //Console.WriteLine("No token found in the cookie.");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Redirect to the login page
                context.Response.Redirect(builder.Configuration["Account:Login"]!);
                context.HandleResponse(); // Avoid the default 401 response
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Hook for further custom checks (e.g., revocation via iat)
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Errore nella validazione del token: {context.Exception.Message}");
                context.Response.Redirect(builder.Configuration["Account:Login"]!);
                return Task.CompletedTask;
            }
        };
    });
#endregion


#region Language Section
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");

var mvcBuilder = builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

var supportedCultures = new[] { "it", "en", "ro", "es", "de", "fr", "pt" };

builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SetDefaultCulture(supportedCultures[0]) // "it"
       .AddSupportedCultures(supportedCultures)
       .AddSupportedUICultures(supportedCultures);

    // (optional) Order providers: ?culture=it, cookie, Accept-Language
    // opt.RequestCultureProviders = new IRequestCultureProvider[] {
    //     new QueryStringRequestCultureProvider(),
    //     new CookieRequestCultureProvider(),
    //     new AcceptLanguageHeaderRequestCultureProvider()
    // };
});
#endregion

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.ContentType = "application/json";
            var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error != null)
            {

                var error = new
                {
                    Message = exceptionFeature.Error.Message,
                    StackTrace = exceptionFeature.Error.StackTrace,
                    Path = exceptionFeature.Path,
                    StatusCode = 500
                };

                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(error);
            }
        });
    });
}

var locOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()
    .Value;

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.TryAdd("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' ws: wss: http://api:8080 http://localhost:5100; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'");
    headers.TryAdd("X-Frame-Options", "DENY");
    headers.TryAdd("X-Content-Type-Options", "nosniff");
    headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

    await next();
});

app.UseRequestLocalization(locOptions);


app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

//app.UseSentryTracing();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<PaymentHub>("/paymentHub");

app.Run();
