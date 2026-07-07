namespace OpenCashFlow.Application.Employees.ResendPin;

public interface IResendEmployeePinUseCase
{
    Task<bool> ExecuteAsync(ResendEmployeePinCommand command, CancellationToken cancellationToken = default);
}
