namespace OpenCashFlow.Application.Employees.CreateEmployee;

public interface ICreateEmployeeUseCase
{
    Task<CreateEmployeeResult> ExecuteAsync(CreateEmployeeCommand command, CancellationToken cancellationToken = default);
}
