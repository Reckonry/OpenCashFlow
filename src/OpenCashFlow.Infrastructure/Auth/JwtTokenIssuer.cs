using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class JwtTokenIssuer(IConfiguration configuration) : IJwtTokenIssuer
{
    public string IssueToken(AuthenticatedUserResult user, Guid tenantId, IReadOnlyList<AuthClaimResult> customClaims, int sessionMinutes)
    {
        var claims = new List<Claim>
        {
            new("Username", user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserFirstName),
            new(ClaimTypes.Surname, user.UserLastName ?? string.Empty),
            new("FullName", user.FullName ?? string.Empty),
            new("Timezone", "Europe/Rome"),
            new("UserID", user.UserID.ToString()),
            new("TenantID", tenantId.ToString()),
            new("UserAvatar", user.UserAvatar ?? string.Empty)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var customClaim in customClaims)
        {
            claims.Add(new Claim(customClaim.Type, customClaim.Value));
        }

        var audiences = configuration.GetSection("JwtSettings:Audience")
            .GetChildren()
            .Select(child => child.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        var audience = audiences != null && audiences.Length > 0
            ? audiences[0]
            : configuration["JwtSettings:Audience"];

        var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:SecretKey"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = configuration["JwtSettings:Issuer"],
            Audience = audience,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(sessionMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
