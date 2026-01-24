using OpenCashFlow.Admin.Services;
using OpenCashFlow.Admin.Services.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Localization;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Reduce Microsoft logs in production
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // support variables from systemd or shell
    
// Add services to the container.
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddTransient<BearerTokenHandler>();
// HttpClient configuration for API access
builder.Services.AddHttpClient("API-Client", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Account:API"]!); // OpenCashFlow.API URL
}).AddHttpMessageHandler<BearerTokenHandler>();

// Add services to the container.
builder.Services.AddScoped<CompanyAPIService>();
builder.Services.AddScoped<AuthenticationAPIService>();
builder.Services.AddScoped<BillingPlansAPIService>();
builder.Services.AddScoped<UserManagementAPIService>();
builder.Services.AddScoped<AuditLogAPIService>();

// Supported cultures for localization
var supportedCultures = new[] { "it", "en", "ro", "es", "de", "fr", "pt" };
builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SetDefaultCulture(supportedCultures[0])
       .AddSupportedCultures(supportedCultures)
       .AddSupportedUICultures(supportedCultures);
});

// Authorization policies
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("GIManagers", policy =>
        policy.RequireClaim(ClaimTypes.Role, "GIManagers"));

// Bearer JWT Authentication (reads token from cookie)
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;

        var audiences = builder.Configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
        var issuers = builder.Configuration.GetSection("JwtSettings:Issuer").Get<string[]?>();
        var keyBytes = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = issuers == null || issuers.Length == 0 ? builder.Configuration["JwtSettings:Issuer"] : null,
            ValidIssuers = issuers != null && issuers.Length > 0 ? issuers : null,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidAudience = audiences == null || audiences.Length == 0 ? builder.Configuration["JwtSettings:Audience"] : null,
            ValidAudiences = audiences != null && audiences.Length > 0 ? audiences : null
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.Request.Path;
                if (path.StartsWithSegments("/Login") || path.StartsWithSegments("/Account/LogIn") || path.StartsWithSegments("/Account/Login"))
                {
                    // Do not attempt authentication on the login page
                    return Task.CompletedTask;
                }

                var token = context.Request.Cookies[global::Shared.Core.Configuration.AuthCookieName];
                if (!string.IsNullOrEmpty(token)) context.Token = token;
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.Response.Redirect(builder.Configuration["Account:Login"]!);
                context.HandleResponse();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var path = context.Request.Path;
                if (path.StartsWithSegments("/Login") || path.StartsWithSegments("/Account/LogIn") || path.StartsWithSegments("/Account/Login"))
                {
                    // Invalid token on the login page: delete cookie and allow navigation
                    context.Response.Cookies.Delete(global::Shared.Core.Configuration.AuthCookieName,
                        new CookieOptions { Domain = builder.Configuration["Account:CookieDomain"], Path = "/" });
                    context.NoResult();
                    return Task.CompletedTask;
                }

                context.Response.Redirect(builder.Configuration["Account:Login"]!);
                return Task.CompletedTask;
            }
        };
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Localization pipeline
var locOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()
    .Value;
app.UseRequestLocalization(locOptions);

// Serve static files (CSS/JS/images) from wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    }
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
