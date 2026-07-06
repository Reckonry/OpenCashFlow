using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Infrastructure.Cash;

namespace OpenCashFlow.Infrastructure.ApplicationAdapters;

public sealed class CashLedgerWriterAdapter(ICashLedgerRepository cashLedgerRepository) : ICashLedgerReader, ICashLedgerWriter
{
    public Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.ApplyPaymentAsync(tenantId, paymentId, delta, userId, cancellationToken);
    }

    public Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.ReapplyPaymentAsync(tenantId, paymentId, delta, userId, cancellationToken);
    }

    public Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.UpdatePaymentAsync(tenantId, paymentId, originalDelta, newDelta, userId, cancellationToken);
    }

    public Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.VoidPaymentAsync(tenantId, paymentId, originalAmount, userId, cancellationToken);
    }

    public Task<decimal> GetPaymentNetBalanceAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.GetPaymentNetBalanceAsync(tenantId, paymentId, cancellationToken);
    }

    public Task<bool> HasVoidedPaymentAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
    {
        return cashLedgerRepository.HasVoidedPaymentAsync(tenantId, paymentId, cancellationToken);
    }
}
