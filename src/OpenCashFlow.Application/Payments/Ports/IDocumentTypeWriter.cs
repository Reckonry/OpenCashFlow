using OpenCashFlow.Application.Payments.Lookups;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IDocumentTypeWriter
{
    Task<DocumentTypeResult?> CreateAsync(DocumentTypeCreateCommand command, CancellationToken cancellationToken = default);

    Task<DocumentTypeResult?> UpdateAsync(DocumentTypeUpdateCommand command, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default);
}
