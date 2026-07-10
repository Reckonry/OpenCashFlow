using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace OpenCashFlow.Api.AppStart;

public static class AuthAppStart
{
    public static WebApplicationBuilder AppStartConfigureAuth(this WebApplicationBuilder builder)
    {
        var secret = builder.Configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JwtSettings:SecretKey mancante.");
        if (secret.Length < 32)
            throw new InvalidOperationException("JwtSettings:SecretKey deve contenere almeno 32 caratteri.");

        var key = Encoding.ASCII.GetBytes(secret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

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
                ValidAudience = audiences == null || audiences.Length == 0 ? builder.Configuration["JwtSettings:Audience"] : null,
                ValidAudiences = audiences != null && audiences.Length > 0 ? audiences : null
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("InstanceAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "InstanceAdmin"))
            .AddPolicy("CompanyAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "CompanyAdmin"))
            .AddPolicy("CompanyMember", policy => policy.RequireClaim(ClaimTypes.Role, "InstanceAdmin", "CompanyAdmin", "Employee"));

        return builder;
    }
}
