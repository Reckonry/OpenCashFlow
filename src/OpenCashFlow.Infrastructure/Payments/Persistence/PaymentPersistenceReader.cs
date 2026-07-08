using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Persistence;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Payments.Persistence;

public sealed class PaymentPersistenceReader(
    ApplicationDbContext context,
    IPaymentMethodReader paymentMethodReader,
    IDocumentTypeReader documentTypeReader) : IPaymentPersistenceReader, IPaymentReader
{
    public async Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var payment = await context.Payment_DS.AsNoTracking()
            .Where(p => p.PaymentID == paymentId && p.TenantID == tenantId && !p.IsDeleted)
            .Include(p => p.User)
            .Include(p => p.PaymentMethod)
            .Include(p => p.DocumentType)
            .FirstOrDefaultAsync(cancellationToken);

        return payment == null ? null : PaymentPersistenceMapping.ToSnapshot(payment);
    }

    public async Task<PaymentSnapshot?> GetByIdForUpdateAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var payment = await context.Payment_DS
            .Where(p => p.PaymentID == paymentId && p.TenantID == tenantId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return payment == null ? null : PaymentPersistenceMapping.ToSnapshot(payment);
    }

    public async Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var payment = await context.Payment_DS.AsNoTracking()
            .Where(p => p.RequestId == requestId && p.TenantID == tenantId)
            .Include(p => p.User)
            .Include(p => p.PaymentMethod)
            .Include(p => p.DocumentType)
            .FirstOrDefaultAsync(cancellationToken);

        return payment == null ? null : PaymentPersistenceMapping.ToSnapshot(payment);
    }

    public async Task<PaymentMethodSnapshot?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await paymentMethodReader.GetByIdAsync(paymentMethodId, tenantId, cancellationToken);
        return paymentMethod == null
            ? null
            : new PaymentMethodSnapshot(paymentMethod.PaymentMethodId, paymentMethod.Name);
    }

    public async Task<DocumentTypeSnapshot?> GetDocumentTypeByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var documentType = await documentTypeReader.GetByIdAsync(documentTypeId, tenantId, cancellationToken);
        return documentType == null
            ? null
            : new DocumentTypeSnapshot(documentType.DocumentTypeId, documentType.Name);
    }
}
