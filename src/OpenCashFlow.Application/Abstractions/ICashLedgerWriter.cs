namespace OpenCashFlow.Application.Abstractions;

public interface ICashLedgerWriter
{
    Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default);

    Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default);

    Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default);

    Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default);
}
