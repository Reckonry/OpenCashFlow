namespace OpenCashFlow.Application.Abstractions;

public interface IAuditWriter
{
    Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default);

    Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default);

    Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default);
}
