using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using global::Shared.Models;
using global::Shared.Models.DTOs;
using System.Xml.Linq;

namespace OpenCashFlow.API.Controllers
{
    public partial class PaymentController : Controller
    {
        
        [HttpGet("[controller]/DocumentTypes")] // GET /v1/payment/DocumentType
        public async Task<ActionResult<IEnumerable<Payment_DocumentType_Detail_DTO>?>> GetDocumentTypes(CancellationToken cancellationToken)
        {
            var Payments = await _paymentService.GetAllDocumentTypesAsync(cancellationToken);
            return Ok(Payments);
        }

        [HttpGet("[controller]/DocumentType/{DocumentTypeID}")] // GET: /v1/payment/DocumentType/5
        public async Task<ActionResult<Payment_DocumentType_Detail_DTO>> GetDocumentType(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            var detail = await _paymentService.GetDocumentTypeByIdAsync(DocumentTypeID, cancellationToken);
            if (detail == null)
                return NotFound(new ApiResponse<Payment_DocumentType_Detail_DTO>
                    (false, $"Document type with ID '{DocumentTypeID}' not found.", null));

            return Ok(new ApiResponse<Payment_DocumentType_Detail_DTO> (true, string.Empty, detail));
        }

        [HttpPost("[controller]/DocumentType")] // POST: /v1/payment/DocumentType
        public async Task<ActionResult<ApiResponse<Payment_DocumentType_Detail_DTO>>> PostDocumentType([FromBody] Payment_DocumentType_Create_DTO documentType, CancellationToken cancellationToken)
        {
            if (documentType == null) return BadRequest(new ApiResponse<Payment_DocumentType_Create_DTO>( false, "Document type cannot be null"));

            Payment_DocumentType_Create_DTO? createdDocumentType = await _paymentService.AddDocumentTypeAsync(documentType, cancellationToken);
            if (createdDocumentType == null) return BadRequest("Failed to create Document Type");
            if (createdDocumentType == null)
                return BadRequest(new ApiResponse<Payment_DocumentType_Create_DTO> (false, "Failed to create Document Type"));

            var detailDto = await _paymentService.GetDocumentTypeByIdAsync(documentType.DocumentTypeID, cancellationToken);
            if (detailDto == null) return NotFound(new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "Created DocumentType not found"));

            var responseWrapper = new ApiResponse<Payment_DocumentType_Detail_DTO> ( true, "", detailDto );
            return CreatedAtAction( nameof(GetDocumentType), new { DocumentTypeID = detailDto.DocumentTypeID }, responseWrapper );
        }

        [HttpPut("[controller]/DocumentType/{DocumentTypeID}")] // PUT: /v1/payment/DocumentType/5
        public async Task<ActionResult<Payment_DocumentType_Detail_DTO>> PutDocumentType(Guid DocumentTypeID, [FromBody] Payment_DocumentType_Update_DTO DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentTypeID != DocumentType.DocumentTypeID)
                return BadRequest(new ApiResponse<Payment_DocumentType_Update_DTO> (false, "ID mismatch"));

            var updated = await _paymentService.UpdateDocumentTypeAsync(DocumentType, cancellationToken);
            if (updated == null)
            {
                return NotFound(new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "Document type not found"));
            }

            var detailDto = await _paymentService.GetDocumentTypeByIdAsync(updated.DocumentTypeID, cancellationToken);
            if (detailDto == null)
                return NotFound(new ApiResponse<Payment_DocumentType_Detail_DTO>
                (false, "Document type updated, but details were not found"));

            return Ok(new ApiResponse<Payment_DocumentType_Detail_DTO> (true, "", detailDto));
        }

        [HttpDelete("[controller]/DocumentType/{DocumentTypeID}")] // DELETE: /v1/payment/DocumentType/5
        public async Task<ActionResult<ApiResponse<object>>> DeleteDocumentType(Guid DocumentTypeID, CancellationToken cancellationToken)
        {
            var success = await _paymentService.DeleteDocumentTypeAsync(DocumentTypeID, cancellationToken);
            if (!success)
            {
                return NotFound(new ApiResponse<object> (false, "Document type not found"));
            }
            return Ok(new ApiResponse<object> (true, ""));
        }
    }
}
