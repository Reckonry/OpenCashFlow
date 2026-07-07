using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Application.Auth.ResetPassword;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResult> Authenticate(string username, string password, CancellationToken cancellationToken);
        Task<ApiResponse<string?>> AuthenticateFastAsync(HttpContext httpContext, string pin, string FLCookieValue, CancellationToken cancellationToken);
        Task<AuthResult> GenerateFastLoginCookieValueAsync(string username, string password, CancellationToken cancellationToken);
        Task ForgotPasswordAsync(string email, CancellationToken cancellationToken);
        Task ForgotPasswordAsync(Guid UserID, CancellationToken cancellationToken);
        Task ResetPasswordAsync(Guid UserID, string token, string newPassword, CancellationToken cancellationToken);
        Task<ValidateResetTokenResult> ValidateResetTokenAsync(string token, CancellationToken cancellationToken);
        Task<Guid?> GetUserIdFromResetTokenAsync(string token, CancellationToken cancellationToken);
        Task ChangeRequiredPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);
        Task RemovePasswordChangeRequirementAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> CanRefreshTokenAsync(string username, CancellationToken cancellationToken);
        Task<Core_RegistrationResult> RegistrationAsync(Register_DTO registration, CancellationToken cancellationToken);
        Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken);
        Task<bool> ResendConfirmationAsync(string usernameOrEmail, CancellationToken cancellationToken);
        Task<AuthResult> RegenerateTokenWithUpdatedClaimsAsync(Guid userId, CancellationToken cancellationToken);


        Guid GetUserID();
        Guid GetTenantID();
    }
}
