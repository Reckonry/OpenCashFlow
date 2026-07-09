using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashTransferReader
{
    Task<CashTransferResult?> GetByIdAsync(Guid tenantId, Guid cashTransferId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashTransferResult>> GetByAccountAsync(
        Guid tenantId,
        Guid cashAccountId,
        DateOnly? businessDate = null,
        CancellationToken cancellationToken = default);
}
