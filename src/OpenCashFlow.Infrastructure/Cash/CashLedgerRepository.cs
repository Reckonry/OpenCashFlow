using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models.Cash;

namespace OpenCashFlow.Infrastructure.Cash;

public sealed class CashLedgerRepository(ApplicationDbContext db) : ICashLedgerRepository
{
    public Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(tenantId, "Payment", paymentId, paymentId, delta, userId.ToString(), cancellationToken);
    }

    public Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(tenantId, "PaymentReapply", Guid.NewGuid(), paymentId, delta, userId.ToString(), cancellationToken);
    }

    public async Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default)
    {
        var adjustmentDelta = newDelta - originalDelta;
        if (adjustmentDelta == 0m)
        {
            return;
        }

        await ApplyDeltaAsync(tenantId, "PaymentUpdate", Guid.NewGuid(), paymentId, adjustmentDelta, userId.ToString(), cancellationToken);
    }

    public async Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default)
    {
        var netBalance = await GetPaymentNetBalanceAsync(tenantId, paymentId, cancellationToken);
        var reverseDelta = netBalance != 0m ? -netBalance : -originalAmount;

        if (reverseDelta == 0m)
        {
            return;
        }

        await ApplyDeltaAsync(tenantId, "Void", Guid.NewGuid(), paymentId, reverseDelta, userId.ToString(), cancellationToken);
    }

    public async Task<decimal> GetPaymentNetBalanceAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await db.CashLedgers.AsNoTracking()
            .Where(x => x.CompanyId == tenantId &&
                       (x.RefId == paymentId || x.OriginalPaymentId == paymentId))
            .SumAsync(x => (decimal?)x.Delta, cancellationToken) ?? 0m;
    }

    public async Task<bool> HasVoidedPaymentAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await db.CashLedgers.AsNoTracking()
            .AnyAsync(x => x.CompanyId == tenantId &&
                           x.RefType == "Void" &&
                           (x.RefId == paymentId || x.OriginalPaymentId == paymentId),
                cancellationToken);
    }

    private async Task ApplyDeltaAsync(Guid tenantId, string refType, Guid refId, Guid? originalPaymentId, decimal delta, string userId, CancellationToken cancellationToken)
    {
        var exists = await db.CashLedgers.AnyAsync(
            x => x.CompanyId == tenantId && x.RefType == refType && x.RefId == refId,
            cancellationToken);

        if (exists)
        {
            return;
        }

        var balance = await db.CashBalances.FirstOrDefaultAsync(x => x.CompanyId == tenantId, cancellationToken);
        if (balance == null)
        {
            balance = new CashBalance
            {
                CompanyId = tenantId,
                Balance = 0m,
                LastUpdatedUtc = DateTimeOffset.UtcNow
            };
            db.CashBalances.Add(balance);
            await db.SaveChangesAsync(cancellationToken);
        }

        balance.Balance += delta;
        balance.LastUpdatedUtc = DateTimeOffset.UtcNow;
        db.CashBalances.Update(balance);

        db.CashLedgers.Add(new CashLedger
        {
            CompanyId = tenantId,
            RefType = refType,
            RefId = refId,
            OriginalPaymentId = originalPaymentId,
            Delta = delta,
            CreatedBy = userId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
