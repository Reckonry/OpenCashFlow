using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.Ports;

public interface IEmployeeReader
{
    Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<EmployeeDetailResult?> GetEmployeeByIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<EmployeeCredentialSnapshot?> GetEmployeeCredentialAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken = default);
}
