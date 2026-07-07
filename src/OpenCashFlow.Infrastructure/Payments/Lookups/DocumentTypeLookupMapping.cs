using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

internal static class DocumentTypeLookupMapping
{
    public static DocumentTypeListItem ToListItem(Payment_DocumentType_LookUp documentType)
    {
        return new DocumentTypeListItem(
            documentType.DocumentTypeID,
            documentType.TenantID,
            documentType.DocumentTypeName,
            documentType.DocumentTypeDescription,
            documentType.DocumentTypeIcon,
            documentType.Visible,
            documentType.DisplayOrder,
            documentType.IsDeleted,
            documentType.IsDeletedBy,
            documentType.IsDeletedWhy,
            documentType.DateDeleted,
            documentType.CreatedBy,
            documentType.DateIns,
            documentType.EditedBy,
            documentType.DateEdit);
    }

    public static DocumentTypeResult ToResult(Payment_DocumentType_LookUp documentType)
    {
        return new DocumentTypeResult(
            documentType.DocumentTypeID,
            documentType.TenantID,
            documentType.DocumentTypeName,
            documentType.DocumentTypeDescription,
            documentType.DocumentTypeIcon,
            documentType.Visible,
            documentType.DisplayOrder,
            documentType.IsDeleted,
            documentType.IsDeletedBy,
            documentType.IsDeletedWhy,
            documentType.DateDeleted,
            documentType.CreatedBy,
            documentType.DateIns,
            documentType.EditedBy,
            documentType.DateEdit);
    }
}
