using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using global::Shared.Core;
using global::Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OpenCashFlow.Test.Utilities;

public static class JwtTokenGenerator
{
    public static async Task<string> GenerateTokenAsync(
        IServiceProvider serviceProvider,
        Guid userId,
        string role = "User",
        Guid? forcedCompanyId = null,
        int expiresInMinutes = 60,
        int? notBeforeOffsetMinutes = null)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var user = await context.AspNetUser_DS.AsNoTracking().FirstAsync(u => u.UserID == userId);
        var companyId = forcedCompanyId ?? await context.Company_Staff_DS
            .AsNoTracking()
            .Where(cs => cs.UserID == userId)
            .Select(cs => cs.TenantID)
            .FirstAsync();

        var claims = new List<Claim>
        {
            new("Username", user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserFirstName),
            new(ClaimTypes.Surname, user.UserLastName ?? string.Empty),
            new("FullName", user.EmployeeSurnameName ?? string.Empty),
            new("Timezone", "Europe/Rome"),
            new("UserID", user.UserID.ToString()),
            new("TenantID", companyId.ToString()),
            new("UserAvatar", user.UserAvatar ?? string.Empty),
            new(ClaimTypes.Role, role)
        };

        var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:SecretKey"]!);
        var now = DateTime.UtcNow;
        var notBefore = notBeforeOffsetMinutes.HasValue ? now.AddMinutes(notBeforeOffsetMinutes.Value) : now;
        var audiences = configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
        var audience = audiences != null && audiences.Length > 0
            ? audiences[0]
            : configuration["JwtSettings:Audience"];
        var expires = now.AddMinutes(expiresInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore,
            Expires = expires,
            Audience = audience,
            Issuer = configuration["JwtSettings:Issuer"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

