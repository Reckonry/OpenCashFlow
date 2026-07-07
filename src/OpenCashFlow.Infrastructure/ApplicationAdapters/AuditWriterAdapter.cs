using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Infrastructure.Audit;

namespace OpenCashFlow.Infrastructure.ApplicationAdapters;

public sealed class AuditWriterAdapter(IAuditRepository auditRepository) : IAuditWriter
{
    public Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default)
    {
        return auditRepository.WritePaymentCreatedAsync(payment, cancellationToken);
    }

    public Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default)
    {
        return auditRepository.WritePaymentUpdatedAsync(payment, cancellationToken);
    }

    public Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default)
    {
        return auditRepository.WritePaymentDeletedAsync(payment, cancellationToken);
    }
}
