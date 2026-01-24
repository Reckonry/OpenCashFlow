using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using OpenCashFlow.API.Services.Interfaces;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Handles the password reset request via email
        /// </summary>
        /// <param name="email">Email of the user requesting the reset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        {
            // Look up the user in the database
            var user = await _employeeRepository.GetUserAsync(email, cancellationToken);
            if (user == null) return; // Do not expose info for security - same behavior whether the user exists or not

            // Generate a unique reset token
            var token = Guid.NewGuid().ToString("N");

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Save the token in the database with configurable expiration
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m) ? m : 30;
            await _employeeRepository.CreateResetTokenAsync(user.UserID, token, DateTime.UtcNow.AddMinutes(ttlMinutes), cancellationToken);

            // Create the reset link for the frontend
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.cloud";
            var resetLink = $"{appBaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}";

            // Load the HTML template and replace placeholders
            var emailContent = await _emailTemplateService.GetForgotPasswordTemplateAsync(user.UserFirstName, resetLink);

            var emailMessage = new global::Shared.Models.EmailMessage("Reset Password - OpenCashFlow", emailContent)
            {
                FromName = "OpenCashFlow — Password Reset"
            };

            // Send the email using the configured sender
            try
            {
                await _emailSender.SendEmailAsync(emailMessage, user.UserFirstName, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw new Exception("Error sending the email. Please try again later.");
            }
        }
        /// <summary>
        /// Overload of ForgotPassword that accepts UserID instead of email
        /// Used when the user ID is already known
        /// </summary>
        /// <param name="UserID">ID of the user requesting the reset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ForgotPasswordAsync(Guid UserID, CancellationToken cancellationToken)
        {
            // Look up the user in the database by ID
            var user = await _employeeRepository.GetUserByIdAsync(UserID, cancellationToken);
            if (user == null) return; // Do not expose info for security

            // Generate a unique reset token
            var token = Guid.NewGuid().ToString("N");

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Save the token in the database with configurable expiration
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m2) ? m2 : 30;
            await _employeeRepository.CreateResetTokenAsync(UserID, token, DateTime.UtcNow.AddMinutes(ttlMinutes), cancellationToken);

            // Create the reset link for the frontend
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.cloud";
            var resetLink = $"{appBaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}";

            // Load the HTML template and replace placeholders
            var emailContent = await _emailTemplateService.GetForgotPasswordTemplateAsync(user.UserFirstName, resetLink);

            var emailMessage = new global::Shared.Models.EmailMessage("Reset Password - OpenCashFlow", emailContent)
            {
                FromName = "OpenCashFlow — Password Reset"
            };

            // Send the email using the configured sender
            try
            {
                await _emailSender.SendEmailAsync(emailMessage, user.UserFirstName, user.Email);
                _logger.LogInformation("Password reset email sent to UserID {UserID}", UserID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to UserID {UserID}", UserID);
                throw new Exception("Error sending the email. Please try again later.");
            }
        }

        /// <summary>
        /// Completes the password reset process using the token received via email
        /// </summary>
        /// <param name="UserID">User ID</param>
        /// <param name="token">Reset token received via email</param>
        /// <param name="newPassword">New password to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ResetPasswordAsync(Guid UserID, string token, string newPassword, CancellationToken cancellationToken)
        {
            // Verify the token is valid and not expired
            var resetToken = await _employeeRepository.HasValidTokenAsync(UserID, token, cancellationToken);
            if (resetToken == false) throw new Exception("Invalid or expired token");

            // Update the user's password in the database
            await _employeeRepository.UpdatePasswordAsync(UserID, newPassword, cancellationToken);
        }
    }
}
