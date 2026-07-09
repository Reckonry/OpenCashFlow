using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashMovementReader
{
    Task<CashMovementResult?> GetByIdAsync(Guid tenantId, Guid cashMovementId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashMovementResult>> GetByAccountAsync(
        Guid tenantId,
        Guid cashAccountId,
        DateOnly? businessDate = null,
        CancellationToken cancellationToken = default);
}
