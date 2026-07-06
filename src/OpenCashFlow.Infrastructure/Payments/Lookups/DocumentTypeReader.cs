using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using global::Shared.Data;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

public sealed class DocumentTypeReader(ApplicationDbContext context) : IDocumentTypeReader
{
    public async Task<IReadOnlyList<DocumentTypeListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var documentTypes = await context.Payment_DocumentType_DS.AsNoTracking()
            .Where(p => (p.TenantID == tenantId || p.TenantID == null) && !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        return documentTypes.Select(DocumentTypeLookupMapping.ToListItem).ToList();
    }

    public async Task<DocumentTypeResult?> GetByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var documentType = await context.Payment_DocumentType_DS.AsNoTracking()
            .Where(p => (p.TenantID == tenantId || p.TenantID == null) && p.DocumentTypeID == documentTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        return documentType == null ? null : DocumentTypeLookupMapping.ToResult(documentType);
    }
}
