using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.Ports;

public interface IEmployeeWriter
{
    Task<EmployeeDetailResult?> CreateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task UpdateMyProfileAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default);
    Task UpdatePinHashAsync(Guid userId, string pinHash, CancellationToken cancellationToken = default);
}
