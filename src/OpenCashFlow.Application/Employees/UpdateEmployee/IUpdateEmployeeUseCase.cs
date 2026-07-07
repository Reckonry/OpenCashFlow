namespace OpenCashFlow.Application.Employees.UpdateEmployee;

public interface IUpdateEmployeeUseCase
{
    Task<UpdateEmployeeResult> ExecuteAsync(UpdateEmployeeCommand command, CancellationToken cancellationToken = default);
}
