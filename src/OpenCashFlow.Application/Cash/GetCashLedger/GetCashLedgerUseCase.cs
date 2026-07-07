using OpenCashFlow.Application.Cash.Models;
using OpenCashFlow.Application.Cash.Ports;

namespace OpenCashFlow.Application.Cash.GetCashLedger;

public sealed class GetCashLedgerUseCase(ICashReader cashReader) : IGetCashLedgerUseCase
{
    public Task<IReadOnlyList<CashLedgerEntry>> ExecuteAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company id is required", nameof(companyId));
        }

        return cashReader.GetLedgerAsync(
            companyId,
            from,
            to,
            Math.Max(skip, 0),
            Math.Clamp(take, 1, 200),
            cancellationToken);
    }
}
