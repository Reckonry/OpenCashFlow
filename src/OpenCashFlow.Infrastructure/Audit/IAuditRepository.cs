using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Infrastructure.Audit;

public interface IAuditRepository
{
    Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default);

    Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default);

    Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default);
}
