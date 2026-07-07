using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// Refreshes the authentication token for an active user session
        /// </summary>
        /// <returns>A new JWT token with extended expiration</returns>
        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AuthResult>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                // Get all claims from current token
                var claims = User.Claims.ToList();

                // Get username for verification
                var usernameClaim = User.FindFirst("Username")?.Value;

                if (string.IsNullOrEmpty(usernameClaim))
                {
                    _logger.LogWarning("Invalid token claims - username is null or empty");
                    return Unauthorized(new ApiResponse<object>(false, "Invalid token claims"));
                }

                if (!await _authenticationService.CanRefreshTokenAsync(usernameClaim, CancellationToken.None))
                {
                    _logger.LogWarning("User {Username} is not approved or is locked out", usernameClaim);
                    return Unauthorized(new ApiResponse<object>(false, "User not found, not approved, or locked out"));
                }

                // Generate new JWT with extended expiration, reusing existing claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

                var audiences = _configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
                var audience = audiences != null && audiences.Length > 0
                    ? audiences[0]
                    : _configuration["JwtSettings:Audience"];

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Issuer = _configuration["JwtSettings:Issuer"],
                    Audience = audience,
                    IssuedAt = DateTime.UtcNow,
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddMinutes(OpenCashFlow.Contracts.Core.Configuration.WebSessionDurationMinutes),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var newToken = tokenHandler.WriteToken(token);

                // Update the auth cookie with new token and extended expiration
                var expirationTime = DateTimeOffset.UtcNow.AddMinutes(OpenCashFlow.Contracts.Core.Configuration.WebSessionDurationMinutes);

                var cookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                Response.Cookies.Append(OpenCashFlow.Contracts.Core.Configuration.AuthCookieName, newToken, cookieOptions);

                // Update info cookie for JavaScript (contains only expiration timestamp)
                var infoCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = false, // JavaScript can read it
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                Response.Cookies.Append(OpenCashFlow.Contracts.Core.Configuration.AuthCookieName + ".Info",
                    expirationTime.ToUnixTimeSeconds().ToString(),
                    infoCookieOptions);

                var response = new ApiResponse<AuthResult>(true, "Token refreshed successfully",
                    new AuthResult
                    {
                        Success = true,
                        Token = newToken,
                        RequiresPasswordChange = false
                    });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { message = "Internal server error during token refresh" });
            }
        }

        /// <summary>
        /// Regenerates the authentication token with updated user claims from the database.
        /// Use this after profile updates to refresh the token without requiring logout.
        /// </summary>
        /// <returns>A new JWT token with updated claims</returns>
        [HttpPost("regenerate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AuthResult>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RegenerateToken()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserID")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid token claims - UserID is null or invalid");
                    return Unauthorized(new ApiResponse<object>(false, "Invalid token claims"));
                }

                var result = await _authenticationService.RegenerateTokenWithUpdatedClaimsAsync(userId, CancellationToken.None);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to regenerate token for user {UserId}", userId);
                    return StatusCode(500, new ApiResponse<object>(false, "Failed to regenerate token"));
                }

                // Update the auth cookie with new token
                var expirationTime = DateTimeOffset.UtcNow.AddMinutes(OpenCashFlow.Contracts.Core.Configuration.WebSessionDurationMinutes);

                var cookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                Response.Cookies.Append(OpenCashFlow.Contracts.Core.Configuration.AuthCookieName, result.Token!, cookieOptions);

                // Update info cookie for JavaScript
                var infoCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                Response.Cookies.Append(OpenCashFlow.Contracts.Core.Configuration.AuthCookieName + ".Info",
                    expirationTime.ToUnixTimeSeconds().ToString(),
                    infoCookieOptions);

                var response = new ApiResponse<AuthResult>(true, "Token regenerated successfully", result);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating token");
                return StatusCode(500, new { message = "Internal server error during token regeneration" });
            }
        }
    }
}
