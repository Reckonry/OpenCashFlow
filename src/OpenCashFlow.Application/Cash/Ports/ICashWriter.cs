namespace OpenCashFlow.Application.Cash.Ports;

public interface ICashWriter
{
    Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default);
    Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default);
    Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken cancellationToken = default);
    Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken cancellationToken = default);
    Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken cancellationToken = default);
    Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken cancellationToken = default);
    Task RebuildBalanceAsync(Guid companyId, CancellationToken cancellationToken = default);
}
