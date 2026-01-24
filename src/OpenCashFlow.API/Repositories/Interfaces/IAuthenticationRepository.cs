using global::Shared.DTOs;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public interface IAuthenticationRepository
    {
        Task<AspNetUser?> GetUserByUsernameAndPasswordAsync(string username, string password, CancellationToken cancellationToken);
        Task<AspNetUser?> GetUserByCompanyIDAndPinAsync(Guid CompanyID, string Pin, CancellationToken cancellationToken);
        Task<AspNetUser?> GetUserByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken);
        Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken);
        Task<bool> RegisterNewUserCompany(Register_DTO registration, Guid TenantID, Guid UserID, CancellationToken cancellationToken);
        Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken);

        Task<IEnumerable<AspNetUserClaim>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
