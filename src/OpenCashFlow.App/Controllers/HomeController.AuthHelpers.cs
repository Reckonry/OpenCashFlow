using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using global::Shared.Core;

namespace OpenCashFlow.App.Controllers
{
    public partial class HomeController
    {
        private bool TryValidateAuthCookie(out ClaimsPrincipal? principal)
        {
            principal = null;
            var token = HttpContext.Request.Cookies[Configuration.AuthCookieName];
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var audiences = _configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)),
                    ValidAudience = audiences == null || audiences.Length == 0 ? _configuration["JwtSettings:Audience"] : null,
                    ValidAudiences = audiences != null && audiences.Length > 0 ? audiences : null
                };

                SecurityToken validatedToken;
                principal = handler.ValidateToken(token, parameters, out validatedToken);
                return principal != null;
            }
            catch
            {
                // Invalid token: delete the cookie to avoid loops
                HttpContext.Response.Cookies.Delete(Configuration.AuthCookieName,
                    new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
                return false;
            }
        }

        private DateTimeOffset? TryGetJwtExpiration(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var expClaim = jwt.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, JwtRegisteredClaimNames.Exp, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Type, "exp", StringComparison.OrdinalIgnoreCase))?.Value;

                if (long.TryParse(expClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(seconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossibile leggere l'exp dal token JWT.");
            }

            return null;
        }
    }
}
