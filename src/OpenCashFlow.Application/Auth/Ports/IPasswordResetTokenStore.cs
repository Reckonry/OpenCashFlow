using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IPasswordResetTokenStore
{
    Task CreateAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task<bool> HasValidTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<PasswordResetTokenValidation> ValidateAsync(string token, CancellationToken cancellationToken = default);
    Task InvalidateAsync(Guid userId, string token, CancellationToken cancellationToken = default);
}
