using OpenCashFlow.Application.Cash.Models;

namespace OpenCashFlow.Application.Cash.GetCashLedger;

public interface IGetCashLedgerUseCase
{
    Task<IReadOnlyList<CashLedgerEntry>> ExecuteAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
