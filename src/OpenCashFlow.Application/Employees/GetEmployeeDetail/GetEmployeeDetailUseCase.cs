using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.GetEmployeeDetail;

public sealed class GetEmployeeDetailUseCase(IEmployeeReader employeeReader) : IGetEmployeeDetailUseCase
{
    public Task<EmployeeDetailResult?> ExecuteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || tenantId == Guid.Empty)
        {
            return Task.FromResult<EmployeeDetailResult?>(null);
        }

        return employeeReader.GetEmployeeByIdAsync(userId, tenantId, cancellationToken);
    }
}
