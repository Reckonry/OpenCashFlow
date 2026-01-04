using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using global::Shared.Core;

namespace OpenCashFlow.Admin.Controllers
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
                var issuers = _configuration.GetSection("JwtSettings:Issuer").Get<string[]?>();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidIssuer = issuers == null || issuers.Length == 0 ? _configuration["JwtSettings:Issuer"] : null,
                    ValidIssuers = issuers != null && issuers.Length > 0 ? issuers : null,
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
                // Token non valido: rimuovo il cookie per evitare loop
                HttpContext.Response.Cookies.Delete(Configuration.AuthCookieName,
                    new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
                return false;
            }
        }
    }
}
