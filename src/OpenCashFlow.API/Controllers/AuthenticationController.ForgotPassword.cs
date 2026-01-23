using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [HttpPost("forgot-password")]
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
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Token and new password are required" });
                }

                var decodedToken = TryDecodeBase64Url(request.Token) ?? request.Token;

                // Extract UserID from the token (DB/token store) using the decoded token
                var userIdFromToken = await ExtractUserIdFromToken(decodedToken, cancellationToken);
                if (userIdFromToken == null)
                {
                    return BadRequest(new { message = "Token is invalid or expired" });
                }

                await _authenticationService.ResetPasswordAsync(userIdFromToken.Value, decodedToken, request.NewPassword, cancellationToken);
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
                // Test basic dependency injection
                var isEmployeeRepoNull = _employeeRepository == null;
                var isAuthServiceNull = _authenticationService == null;

                _logger.LogInformation("Testing configuration. EmployeeRepo is null: {EmployeeRepoNull}, AuthService is null: {AuthServiceNull}",
                    isEmployeeRepoNull, isAuthServiceNull);

                return Ok(new {
                    employeeRepositoryAvailable = !isEmployeeRepoNull,
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

                var decoded = TryDecodeBase64Url(token) ?? token;
                var (isValid, isExpired, user) = await _employeeRepository.ValidateResetTokenAsync(decoded, cancellationToken);

                return Ok(new {
                    isValid = isValid,
                    isExpired = isExpired,
                    message = isValid ? "Token is valid" :
                             isExpired ? "Token expired" : "Token is invalid"
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
                return await _employeeRepository.GetUserIdFromResetTokenAsync(token, cancellationToken);
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
