using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IUserCredentialReader
{
    Task<AuthUserCredential?> GetByEmailOrUserNameAsync(string userInput, CancellationToken cancellationToken = default);
    Task<AuthUserCredential?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
