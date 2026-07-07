using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.Cash.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities.Cash;

namespace OpenCashFlow.Infrastructure.Cash;

public sealed class CashWriter(ApplicationDbContext db, ILogger<CashWriter> logger) : ICashWriter
{
    public Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(companyId, "Payment", paymentId, paymentId, +amount, userId, null, cancellationToken);
    }

    public Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(companyId, "PaymentReapply", Guid.NewGuid(), paymentId, +amount, userId, null, cancellationToken);
    }

    public Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(companyId, "Refund", paymentId, paymentId, -amount, userId, null, cancellationToken);
    }

    public async Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken cancellationToken = default)
    {
        var adjustmentDelta = newDelta - originalDelta;
        if (adjustmentDelta != 0m)
        {
            await ApplyDeltaAsync(companyId, "PaymentUpdate", Guid.NewGuid(), paymentId, adjustmentDelta, userId, null, cancellationToken);
        }
    }

    public async Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken cancellationToken = default)
    {
        var netBalance = await db.CashLedgers.AsNoTracking()
            .Where(x => x.CompanyId == companyId &&
                       (x.RefId == paymentId || x.OriginalPaymentId == paymentId))
            .SumAsync(x => (decimal?)x.Delta, cancellationToken) ?? 0m;

        logger.LogInformation("VoidAsync called for payment {PaymentID}. NetBalance: {NetBalance}. Original: {Original}",
            paymentId, netBalance, originalAmount);

        var reverseDelta = netBalance != 0m ? -netBalance : -originalAmount;
        if (reverseDelta != 0m)
        {
            await ApplyDeltaAsync(companyId, "Void", Guid.NewGuid(), paymentId, reverseDelta, userId, null, cancellationToken);
        }
    }

    public Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required", nameof(reason));
        }

        return ApplyDeltaAsync(companyId, "Adjustment", Guid.NewGuid(), null, delta, userId, reason, cancellationToken);
    }

    public async Task RebuildBalanceAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var sum = await db.CashLedgers
                .Where(x => x.CompanyId == companyId)
                .SumAsync(x => (decimal?)x.Delta, cancellationToken) ?? 0m;

            var balance = await db.CashBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);
            if (balance == null)
            {
                balance = new CashBalance
                {
                    CompanyId = companyId,
                    Balance = sum,
                    LastUpdatedUtc = DateTimeOffset.UtcNow
                };
                db.CashBalances.Add(balance);
            }
            else
            {
                balance.Balance = sum;
                balance.LastUpdatedUtc = DateTimeOffset.UtcNow;
                db.CashBalances.Update(balance);
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ApplyDeltaAsync(
        Guid companyId,
        string refType,
        Guid refId,
        Guid? originalPaymentId,
        decimal delta,
        string userId,
        string? reason,
        CancellationToken cancellationToken)
    {
        var exists = await db.CashLedgers.AnyAsync(
            x => x.CompanyId == companyId && x.RefType == refType && x.RefId == refId,
            cancellationToken);

        if (exists)
        {
            return;
        }

        var externalTransaction = db.Database.CurrentTransaction != null;
        var attempt = 0;

        while (true)
        {
            attempt++;
            var tx = externalTransaction ? null : await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var balance = await db.CashBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);
                if (balance == null)
                {
                    balance = new CashBalance
                    {
                        CompanyId = companyId,
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
                    CompanyId = companyId,
                    RefType = refType,
                    RefId = refId,
                    OriginalPaymentId = originalPaymentId,
                    Delta = delta,
                    Reason = reason,
                    CreatedBy = userId,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                });

                await db.SaveChangesAsync(cancellationToken);

                if (tx != null)
                {
                    await tx.CommitAsync(cancellationToken);
                }

                break;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogWarning(ex, "Concurrency conflict applying cash delta. Attempt {Attempt}", attempt);
                if (tx != null)
                {
                    await tx.RollbackAsync(cancellationToken);
                }

                if (attempt >= 2)
                {
                    throw;
                }

                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(cancellationToken);
                }
            }
            catch (DbUpdateException)
            {
                if (tx != null)
                {
                    await tx.RollbackAsync(cancellationToken);
                }

                var already = await db.CashLedgers.AnyAsync(
                    x => x.CompanyId == companyId && x.RefType == refType && x.RefId == refId,
                    cancellationToken);

                if (already)
                {
                    return;
                }

                throw;
            }
            catch
            {
                if (tx != null)
                {
                    await tx.RollbackAsync(cancellationToken);
                }

                throw;
            }
            finally
            {
                tx?.Dispose();
            }
        }
    }
}
