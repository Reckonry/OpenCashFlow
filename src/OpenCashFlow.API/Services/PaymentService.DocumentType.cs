using OpenCashFlow.Application.Payments.DocumentTypes;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Contracts.DTOs.Payments;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<IEnumerable<Payment_DocumentType_List_DTO>?> GetAllDocumentTypesAsync(CancellationToken cancellationToken)
        {
            var documentTypes = await _getDocumentTypesUseCase.ExecuteAsync(
                new GetDocumentTypesQuery(_authenticationService.GetTenantID()),
                cancellationToken);

            return documentTypes.Select(ToDocumentTypeListDto).ToList();
        }

        public async Task<Payment_DocumentType_Detail_DTO?> GetDocumentTypeByIdAsync(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            var documentType = await _getDocumentTypeDetailUseCase.ExecuteAsync(
                new GetDocumentTypeDetailQuery(DocumentTypeID, _authenticationService.GetTenantID()),
                cancellationToken);

            return documentType == null ? null : ToDocumentTypeDetailDto(documentType);
        }

        public async Task<Payment_DocumentType_Create_DTO?> AddDocumentTypeAsync(Payment_DocumentType_Create_DTO DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null) return null;

            var command = new DocumentTypeCreateCommand(
                DocumentType.DocumentTypeID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                DocumentType.DocumentTypeName,
                DocumentType.DocumentTypeDescription,
                DocumentType.DocumentTypeIcon,
                DocumentType.Visible,
                DocumentType.DisplayOrder);

            var created = await _createDocumentTypeUseCase.ExecuteAsync(command, cancellationToken);
            return created == null ? null : ToDocumentTypeCreateDto(created);
        }

        public async Task<Payment_DocumentType_Update_DTO?> UpdateDocumentTypeAsync(Payment_DocumentType_Update_DTO DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null) return null;

            var command = new DocumentTypeUpdateCommand(
                DocumentType.DocumentTypeID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                DocumentType.DocumentTypeName,
                DocumentType.DocumentTypeDescription,
                DocumentType.DocumentTypeIcon,
                DocumentType.Visible,
                DocumentType.DisplayOrder);

            var updated = await _updateDocumentTypeUseCase.ExecuteAsync(command, cancellationToken);
            return updated == null ? null : ToDocumentTypeUpdateDto(updated);
        }

        public async Task<bool> DeleteDocumentTypeAsync(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            return await _deleteDocumentTypeUseCase.ExecuteAsync(
                new DeleteDocumentTypeCommand(DocumentTypeID, _authenticationService.GetTenantID()),
                cancellationToken);
        }
    }
}
