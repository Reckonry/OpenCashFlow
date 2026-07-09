using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashSessionReader
{
    Task<CashSessionResult?> GetByIdAsync(Guid tenantId, Guid cashSessionId, CancellationToken cancellationToken = default);

    Task<CashSessionResult?> GetCurrentAsync(
        Guid tenantId,
        Guid cashAccountId,
        DateOnly businessDate,
        CancellationToken cancellationToken = default);
}
