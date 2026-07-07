using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Cash.Models;
using OpenCashFlow.Application.Cash.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Cash;

public sealed class CashReader(ApplicationDbContext db) : ICashReader
{
    public async Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var balance = await db.CashBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        return balance?.Balance ?? 0m;
    }

    public async Task<IReadOnlyList<CashLedgerEntry>> GetLedgerAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = db.CashLedgers.AsNoTracking().Where(x => x.CompanyId == companyId);

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= to.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(x => new CashLedgerEntry(
                x.Id,
                x.CompanyId,
                x.RefType,
                x.RefId,
                x.OriginalPaymentId,
                x.Delta,
                x.Reason,
                x.CreatedBy,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
