using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Register;

public interface IRegisterUseCase
{
    Task<RegistrationResult> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken = default);
}
