namespace OpenCashFlow.Application.Abstractions;

public interface IPaymentReader
{
    Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<PaymentMethodSnapshot?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default);
}
