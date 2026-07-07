namespace OpenCashFlow.Application.Employees.DeleteEmployee;

public interface IDeleteEmployeeUseCase
{
    Task<bool> ExecuteAsync(Guid userId, Guid tenantId, Guid currentUserId, CancellationToken cancellationToken = default);
}
