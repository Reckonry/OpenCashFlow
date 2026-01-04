using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<IEnumerable<Payment_DocumentType_List_DTO>?> GetAllDocumentTypesAsync(CancellationToken cancellationToken)
        {
            return _mapper.Map<IEnumerable<Payment_DocumentType_List_DTO>>(
                await _paymentRepository.GetAllDocumentTypesAsync(_authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Payment_DocumentType_Detail_DTO?> GetDocumentTypeByIdAsync(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            return _mapper.Map<Payment_DocumentType_Detail_DTO>(await _paymentRepository.GetDocumentTypeByIdAsync(DocumentTypeID, _authenticationService.GetTenantID(), cancellationToken));
        }

        public async Task<Payment_DocumentType_Create_DTO?> AddDocumentTypeAsync(Payment_DocumentType_Create_DTO DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null) return null;
            DocumentType.TenantID = _authenticationService.GetTenantID();
            DocumentType.CreatedBy = _authenticationService.GetUserID();
            return _mapper.Map<Payment_DocumentType_Create_DTO>(
                await _paymentRepository.AddPaymentDocumentTypeAsync(_mapper.Map<Payment_DocumentType_LookUp>(DocumentType), cancellationToken));
        }

        public async Task<Payment_DocumentType_Update_DTO?> UpdateDocumentTypeAsync(Payment_DocumentType_Update_DTO DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null) return null;
            var DocumentTypeTBE = await _paymentRepository.GetDocumentTypeByIdAsync(DocumentType.DocumentTypeID, _authenticationService.GetTenantID(), cancellationToken);
            if (DocumentTypeTBE == null) return null;

            DocumentTypeTBE.DisplayOrder = DocumentType.DisplayOrder;
            DocumentTypeTBE.DocumentTypeDescription = DocumentType.DocumentTypeDescription;
            DocumentTypeTBE.DocumentTypeIcon = DocumentType.DocumentTypeIcon;
            DocumentTypeTBE.DocumentTypeName = DocumentType.DocumentTypeName;
            DocumentTypeTBE.Visible = DocumentType.Visible;
            DocumentTypeTBE.DateEdit = DateTime.UtcNow;
            DocumentTypeTBE.EditedBy = _authenticationService.GetUserID();
            return _mapper.Map<Payment_DocumentType_Update_DTO>(await _paymentRepository.UpdateDocumentTypeAsync(_mapper.Map<Payment_DocumentType_LookUp>(DocumentTypeTBE), cancellationToken)); 
        }

        public async Task<bool> DeleteDocumentTypeAsync(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            return await _paymentRepository.DeleteDocumentTypeAsync(DocumentTypeID, _authenticationService.GetTenantID(), cancellationToken);
        }
    }
}
