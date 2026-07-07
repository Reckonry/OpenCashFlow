using OpenCashFlow.Application.Cash.Models;

namespace OpenCashFlow.Application.Cash.Ports;

public interface ICashReader
{
    Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashLedgerEntry>> GetLedgerAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
