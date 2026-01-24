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
                OnMessageReceived = context =>
                {
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

        return builder;
    }
}