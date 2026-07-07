using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.Persistence;

public interface IPaymentPersistenceWriter
{
    Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default);

    Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default);
}
