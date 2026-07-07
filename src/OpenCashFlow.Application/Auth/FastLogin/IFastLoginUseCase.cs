using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.FastLogin;

public interface IFastLoginUseCase
{
    Task<FastLoginResult> ExecuteAsync(FastLoginCommand command, CancellationToken cancellationToken = default);
}
