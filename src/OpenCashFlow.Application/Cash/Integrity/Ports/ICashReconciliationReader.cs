using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashReconciliationReader
{
    Task<CashReconciliationResult?> GetBySessionAsync(Guid tenantId, Guid cashSessionId, CancellationToken cancellationToken = default);
}
