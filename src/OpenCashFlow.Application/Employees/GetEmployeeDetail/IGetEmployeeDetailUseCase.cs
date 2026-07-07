using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.GetEmployeeDetail;

public interface IGetEmployeeDetailUseCase
{
    Task<EmployeeDetailResult?> ExecuteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
