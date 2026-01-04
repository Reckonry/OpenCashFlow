using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models.Cash;

namespace OpenCashFlow.API.Services
{
    public class CashService(ApplicationDbContext db, ILogger<CashService> logger) : Interfaces.ICashService
    {
        private readonly ApplicationDbContext _db = db;
        private readonly ILogger<CashService> _logger = logger;

        public Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => ApplyDeltaAsync(companyId, "Payment", paymentId, paymentId, +amount, userId, null, ct);

        public Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => ApplyDeltaAsync(companyId, "PaymentReapply", Guid.NewGuid(), paymentId, +amount, userId, null, ct);

        public Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => ApplyDeltaAsync(companyId, "Refund", paymentId, paymentId, -amount, userId, null, ct);

        public async Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken ct)
        {
            // Calculate the adjustment needed: difference between new and original
            var adjustmentDelta = newDelta - originalDelta;

            // Only apply if there's an actual change
            if (adjustmentDelta != 0m)
            {
                // Apply the adjustment with a unique RefId to avoid idempotency issues
                await ApplyDeltaAsync(companyId, "PaymentUpdate", Guid.NewGuid(), paymentId, adjustmentDelta, userId, null, ct);
            }
        }

        public async Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken ct)
        {
            // Calculate the current net balance for this payment by summing all related entries
            // Use OriginalPaymentId to find all entries related to this payment
            var netBalance = await _db.CashLedgers.AsNoTracking()
                .Where(x => x.CompanyId == companyId &&
                           (x.RefId == paymentId || x.OriginalPaymentId == paymentId))
                .SumAsync(x => (decimal?)x.Delta, ct) ?? 0m;

            _logger.LogInformation("VoidAsync called for payment {PaymentID}. NetBalance: {NetBalance}, Original: {Original}",
                paymentId, netBalance, originalAmount);

            // If there's an active balance, reverse it; otherwise reverse the original amount
            var reverseDelta = netBalance != 0m ? -netBalance : -originalAmount;

            // Only apply void if there's something to void
            if (reverseDelta != 0m)
            {
                _logger.LogInformation("Applying void with delta {Delta}", reverseDelta);
                // Use Guid.NewGuid() to allow multiple voids for the same payment
                // Track original payment via OriginalPaymentId
                await ApplyDeltaAsync(companyId, "Void", Guid.NewGuid(), paymentId, reverseDelta, userId, null, ct);
            }
            else
            {
                _logger.LogInformation("Skipping void - reverseDelta is 0");
            }
        }

        public Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason is required", nameof(reason));
            return ApplyDeltaAsync(companyId, "Adjustment", Guid.NewGuid(), null, delta, userId, reason, ct);
        }

        public async Task RebuildBalanceAsync(Guid companyId, CancellationToken ct)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var sum = await _db.CashLedgers
                    .Where(x => x.CompanyId == companyId)
                    .SumAsync(x => (decimal?)x.Delta, ct) ?? 0m;

                var bal = await _db.CashBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId, ct);
                if (bal == null)
                {
                    bal = new CashBalance { CompanyId = companyId, Balance = sum, LastUpdatedUtc = DateTimeOffset.UtcNow };
                    _db.CashBalances.Add(bal);
                }
                else
                {
                    bal.Balance = sum;
                    bal.LastUpdatedUtc = DateTimeOffset.UtcNow;
                    _db.CashBalances.Update(bal);
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken ct)
        {
            var bal = await _db.CashBalances.AsNoTracking().FirstOrDefaultAsync(x => x.CompanyId == companyId, ct);
            return bal?.Balance ?? 0m;
        }

        public async Task<IReadOnlyList<CashLedger>> GetLedgerAsync(Guid companyId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken ct)
        {
            var query = _db.CashLedgers.AsNoTracking().Where(x => x.CompanyId == companyId);
            if (from.HasValue) query = query.Where(x => x.CreatedAtUtc >= from.Value);
            if (to.HasValue) query = query.Where(x => x.CreatedAtUtc <= to.Value);
            return await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        private async Task ApplyDeltaAsync(Guid companyId, string refType, Guid refId, Guid? originalPaymentId, decimal delta, string userId, string? reason, CancellationToken ct)
        {
            // Idempotency: if already recorded, do nothing
            var exists = await _db.CashLedgers.AnyAsync(x => x.CompanyId == companyId && x.RefType == refType && x.RefId == refId, ct);
            if (exists) return;

            // Check if there's already an active transaction (from PaymentService)
            var currentTransaction = _db.Database.CurrentTransaction;
            var externalTransaction = currentTransaction != null;

            var attempt = 0;
            while (true)
            {
                attempt++;
                // Only create a new transaction if there isn't one already
                var tx = externalTransaction ? null : await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    var balance = await _db.CashBalances.FirstOrDefaultAsync(x => x.CompanyId == companyId, ct);
                    if (balance == null)
                    {
                        balance = new CashBalance
                        {
                            CompanyId = companyId,
                            Balance = 0m,
                            LastUpdatedUtc = DateTimeOffset.UtcNow,
                        };
                        _db.CashBalances.Add(balance);
                        await _db.SaveChangesAsync(ct);
                    }

                    balance.Balance += delta;
                    balance.LastUpdatedUtc = DateTimeOffset.UtcNow;
                    _db.CashBalances.Update(balance);

                    var ledger = new CashLedger
                    {
                        CompanyId = companyId,
                        RefType = refType,
                        RefId = refId,
                        OriginalPaymentId = originalPaymentId,
                        Delta = delta,
                        Reason = reason,
                        CreatedBy = userId,
                        CreatedAtUtc = DateTimeOffset.UtcNow
                    };
                    _db.CashLedgers.Add(ledger);

                    await _db.SaveChangesAsync(ct);

                    // Only commit if we created the transaction
                    if (tx != null)
                    {
                        await tx.CommitAsync(ct);
                    }
                    break;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency conflict applying cash delta. Attempt {Attempt}", attempt);
                    if (tx != null) await tx.RollbackAsync(ct);
                    if (attempt >= 2) throw; // small retry once
                    // reload tracked entities and retry
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync(ct);
                    }
                }
                catch (DbUpdateException)
                {
                    if (tx != null) await tx.RollbackAsync(ct);
                    // Could be unique constraint for idempotency; double-check and return if so
                    var already = await _db.CashLedgers.AnyAsync(x => x.CompanyId == companyId && x.RefType == refType && x.RefId == refId, ct);
                    if (already) return;
                    throw;
                }
                catch
                {
                    if (tx != null) await tx.RollbackAsync(ct);
                    throw;
                }
                finally
                {
                    tx?.Dispose();
                }
            }
        }
    }
}
