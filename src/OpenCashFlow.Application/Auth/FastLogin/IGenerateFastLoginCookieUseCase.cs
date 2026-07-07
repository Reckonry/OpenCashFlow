using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.FastLogin;

public interface IGenerateFastLoginCookieUseCase
{
    Task<FastLoginCookieResult> ExecuteAsync(GenerateFastLoginCookieCommand command, CancellationToken cancellationToken = default);
}
