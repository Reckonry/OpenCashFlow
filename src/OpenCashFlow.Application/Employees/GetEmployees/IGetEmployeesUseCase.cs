using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.GetEmployees;

public interface IGetEmployeesUseCase
{
    Task<IReadOnlyList<EmployeeListItem>> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
