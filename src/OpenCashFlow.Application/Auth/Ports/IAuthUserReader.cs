using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IAuthUserReader
{
    Task<AuthenticatedUserResult?> GetByUsernameEmailOrPhoneAsync(string userInput, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserResult?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserResult?> GetByTenantAndPinAsync(Guid tenantId, string pin, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetCompanySecretAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CompanyExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthClaimResult>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken = default);
}
