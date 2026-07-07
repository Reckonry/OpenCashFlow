using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.DeleteEmployee;

public sealed class DeleteEmployeeUseCase(IEmployeeReader employeeReader, IEmployeeWriter employeeWriter) : IDeleteEmployeeUseCase
{
    public async Task<bool> ExecuteAsync(Guid userId, Guid tenantId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || tenantId == Guid.Empty)
        {
            return false;
        }

        if (userId == currentUserId)
        {
            throw new InvalidOperationException("You cannot delete your own account");
        }

        var employee = await employeeReader.GetEmployeeByIdAsync(userId, tenantId, cancellationToken);
        if (employee is null)
        {
            return false;
        }

        return await employeeWriter.SoftDeleteAsync(userId, tenantId, cancellationToken);
    }
}
