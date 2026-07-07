namespace OpenCashFlow.Application.Abstractions;

public interface ICashLedgerReader
{
    Task<decimal> GetPaymentNetBalanceAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);

    Task<bool> HasVoidedPaymentAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default);
}
