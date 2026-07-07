using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IAuthUserWriter
{
    Task<bool> CreateRegisteredCompanyAndUserAsync(RegistrationDraft draft, Guid tenantId, Guid userId, string passwordSalt, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> ConfirmAccountAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
