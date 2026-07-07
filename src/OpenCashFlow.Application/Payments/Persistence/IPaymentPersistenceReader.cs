using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.Persistence;

public interface IPaymentPersistenceReader
{
    Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<PaymentSnapshot?> GetByIdForUpdateAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default);
}
