namespace OpenCashFlow.Application.Abstractions;

public interface IPaymentWriter
{
    Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default);

    Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default);
}
