using OpenCashFlow.Application.Auth.ForgotPassword;
using OpenCashFlow.Application.Auth.ResetPassword;
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
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m) ? m : 30;
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.local";
            var result = await _forgotPasswordUseCase.ExecuteAsync(new ForgotPasswordCommand(email, null, appBaseUrl, ttlMinutes), cancellationToken);

            if (result.TokenCreated && result.UserID.HasValue)
            {
                await WriteAuthenticationAuditAsync(OpenCashFlow.Contracts.Audit.AuditEventType.PasswordReset, "PasswordResetRequested", result.UserName, result.UserID.Value, null, null, cancellationToken);
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
            var ttlMinutes = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_RESET_TOKEN_MINUTES"), out var m2) ? m2 : 30;
            var appBaseUrl = _configuration["AppUrl"] ?? "https://app.opencashflow.local";
            var result = await _forgotPasswordUseCase.ExecuteAsync(new ForgotPasswordCommand(null, UserID, appBaseUrl, ttlMinutes), cancellationToken);
            if (result.TokenCreated)
            {
                await WriteAuthenticationAuditAsync(OpenCashFlow.Contracts.Audit.AuditEventType.PasswordReset, "PasswordResetRequested", result.UserName, UserID, null, null, cancellationToken);
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
            var result = await _resetPasswordUseCase.ExecuteAsync(new ResetPasswordCommand(UserID, token, newPassword), cancellationToken);
            if (!result.Success)
            {
                throw new Exception(result.Error ?? "Invalid or expired token");
            }

            await WriteAuthenticationAuditAsync(OpenCashFlow.Contracts.Audit.AuditEventType.PasswordChanged, "PasswordResetCompleted", null, UserID, null, null, cancellationToken);
        }

        public Task<ValidateResetTokenResult> ValidateResetTokenAsync(string token, CancellationToken cancellationToken)
        {
            return _validateResetTokenUseCase.ExecuteAsync(new ValidateResetTokenCommand(token), cancellationToken);
        }

        public async Task<Guid?> GetUserIdFromResetTokenAsync(string token, CancellationToken cancellationToken)
        {
            var decodedToken = _passwordResetTokenGenerator.DecodeTokenOrPassthrough(token);
            return await _passwordResetTokenStore.GetUserIdFromTokenAsync(decodedToken, cancellationToken);
        }

        public async Task ChangeRequiredPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userCredentialReader.GetByIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Utente non trovato");

            await _userPasswordWriter.UpdatePasswordHashAsync(userId, _employeeCredentialService.HashSecret(newPassword, user.PasswordSalt), cancellationToken);
            await WriteAuthenticationAuditAsync(OpenCashFlow.Contracts.Audit.AuditEventType.PasswordChanged, "PasswordChanged", null, userId, null, null, cancellationToken);
        }

        public async Task RemovePasswordChangeRequirementAsync(Guid userId, CancellationToken cancellationToken)
        {
            await _userPasswordWriter.RemovePasswordChangeRequirementAsync(userId, cancellationToken);
        }

        public async Task<bool> CanRefreshTokenAsync(string username, CancellationToken cancellationToken)
        {
            var user = await _userCredentialReader.GetByEmailOrUserNameAsync(username, cancellationToken);
            return user is not null && user.IsApproved && !user.LockoutEnabled;
        }
    }
}
