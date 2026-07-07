using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.GetEmployees;

public sealed class GetEmployeesUseCase(IEmployeeReader employeeReader) : IGetEmployeesUseCase
{
    public Task<IReadOnlyList<EmployeeListItem>> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Task.FromResult<IReadOnlyList<EmployeeListItem>>(Array.Empty<EmployeeListItem>());
        }

        return employeeReader.GetEmployeesAsync(tenantId, cancellationToken);
    }
}
