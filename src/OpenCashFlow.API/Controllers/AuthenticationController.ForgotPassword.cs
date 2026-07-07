using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [HttpPost("forgot-password")]
        [EnableRateLimiting("auth-limiter")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                await _authenticationService.ForgotPasswordAsync(request.Email, cancellationToken);
                return Ok(new { message = "If the account exists, you will receive an email shortly." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword for email {Email}", request.Email);
                return StatusCode(500, new { message = "Internal server error. Please try again later." });
            }
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("auth-limiter")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Token and new password are required" });
                }

                var userIdFromToken = await ExtractUserIdFromToken(request.Token, cancellationToken);
                if (userIdFromToken == null)
                {
                    return BadRequest(new { message = "Token is invalid or expired" });
                }

                await _authenticationService.ResetPasswordAsync(userIdFromToken.Value, request.Token, request.NewPassword, cancellationToken);
                return Ok(new { message = "Password reset completed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPassword");
                return StatusCode(500, new { message = "Internal server error. Please try again later." });
            }
        }

        [HttpGet("test-email-config")]
        public IActionResult TestEmailConfig()
        {
            try
            {
                var isAuthServiceNull = _authenticationService == null;

                _logger.LogInformation("Testing configuration. AuthService is null: {AuthServiceNull}", isAuthServiceNull);

                return Ok(new {
                    authenticationServiceAvailable = !isAuthServiceNull,
                    message = "Configuration test completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing configuration");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(string token, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { message = "Token is required" });
                }

                var validation = await _authenticationService.ValidateResetTokenAsync(token, cancellationToken);

                return Ok(new {
                    isValid = validation.IsValid,
                    isExpired = validation.IsExpired,
                    message = validation.IsValid ? "Token is valid" :
                             validation.IsExpired ? "Token expired" : "Token is invalid"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token");
                return StatusCode(500, new { message = "Internal server error. Please try again later." });
            }
        }

        private async Task<Guid?> ExtractUserIdFromToken(string token, CancellationToken cancellationToken)
        {
            try
            {
                return await _authenticationService.GetUserIdFromResetTokenAsync(token, cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        private static string? TryDecodeBase64Url(string token)
        {
            try
            {
                // Normalize Base64Url (replace '-' -> '+', '_' -> '/') and pad with '=' if needed
                string s = token.Replace('-', '+').Replace('_', '/');
                switch (s.Length % 4)
                {
                    case 2: s += "=="; break;
                    case 3: s += "="; break;
                }
                var bytes = System.Convert.FromBase64String(s);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null; // not a base64url token; return null so callers can fallback
            }
        }

    }
}
