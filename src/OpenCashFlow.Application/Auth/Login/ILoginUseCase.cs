using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Login;

public interface ILoginUseCase
{
    Task<LoginResult> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default);
}
