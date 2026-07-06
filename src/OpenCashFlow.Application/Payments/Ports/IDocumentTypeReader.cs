using OpenCashFlow.Application.Payments.Lookups;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IDocumentTypeReader
{
    Task<IReadOnlyList<DocumentTypeListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<DocumentTypeResult?> GetByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default);
}
