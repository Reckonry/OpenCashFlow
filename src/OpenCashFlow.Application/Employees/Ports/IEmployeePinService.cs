namespace OpenCashFlow.Application.Employees.Ports;

public interface IEmployeePinService
{
    Task<string> GenerateUniquePinAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
