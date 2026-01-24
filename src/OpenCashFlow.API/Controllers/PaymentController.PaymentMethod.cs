using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.API.Controllers
{
    public partial class PaymentController : Controller
    {
        
        [HttpGet("[controller]/PaymentMethods")] // GET /v1/payment/PaymentMethods
        public async Task<ActionResult<IEnumerable<Payment_Method_List_DTO>?>> GetPaymentMethods(CancellationToken cancellationToken)
        {
            var Payments = await _paymentService.GetAllPaymentMethodsAsync(cancellationToken);
            return Ok(Payments);
        }

        [HttpGet("[controller]/PaymentMethod/{PaymentMethodID}")] // GET: /v1/payment/PaymentsMethods/5
        public async Task<ActionResult<Payment_Method_Detail_DTO>> GetPaymentMethod(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            var detail = await _paymentService.GetPaymentMethodByIdAsync(PaymentMethodID, cancellationToken);
            if (detail == null)
                return NotFound(new ApiResponse<Payment_Method_Detail_DTO>
                    (false, $"Payment method with ID '{PaymentMethodID}' not found.", null));

            return Ok(new ApiResponse<Payment_Method_Detail_DTO> (true, string.Empty, detail));
        }

        [HttpPost("[controller]/PaymentMethod")] // POST: /v1/payment/PaymentsMethods
        public async Task<ActionResult<ApiResponse<Payment_Method_Detail_DTO>>> PostPaymentMethod([FromBody] Payment_Method_Create_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null) return BadRequest(new ApiResponse<Payment_Method_Create_DTO>( false, "Payment method cannot be null"));

            Payment_Method_Create_DTO? createdPaymentMethod = await _paymentService.AddPaymentMethodAsync(paymentMethod, cancellationToken);
            if (createdPaymentMethod == null) return BadRequest("Failed to create payment method");
            if (createdPaymentMethod == null)
                return BadRequest(new ApiResponse<Payment_Method_Create_DTO> (false, "Failed to create payment method"));

            var detailDto = await _paymentService.GetPaymentMethodByIdAsync(createdPaymentMethod.PaymentMethodID, cancellationToken);
            if (detailDto == null) return NotFound(new ApiResponse<Payment_Method_Detail_DTO> (false, "Created payment method not found"));

            var responseWrapper = new ApiResponse<Payment_Method_Detail_DTO> ( true, "", detailDto );
            return CreatedAtAction( nameof(GetPaymentMethod), new { PaymentMethodID = detailDto.PaymentMethodID }, responseWrapper );
        }

        [HttpPut("[controller]/PaymentMethod/{PaymentMethodID}")] // PUT: /v1/payment/PaymentsMethods/5
        public async Task<ActionResult<Payment_Method_Detail_DTO>> PutPaymentMethod(Guid PaymentMethodID, [FromBody] Payment_Method_Update_DTO paymentMethod, CancellationToken cancellationToken)
        {
            if (PaymentMethodID != paymentMethod.PaymentMethodID)
                return BadRequest(new ApiResponse<Payment_Method_Update_DTO> (false, "ID mismatch"));

            var updated = await _paymentService.UpdatePaymentMethodAsync(paymentMethod, cancellationToken);
            if (updated == null)
            {
                return NotFound(new ApiResponse<Payment_Method_Detail_DTO> (false, "Payment method not found"));
            }

            var detailDto = await _paymentService.GetPaymentMethodByIdAsync(updated.PaymentMethodID, cancellationToken);
            if (detailDto == null)
                return NotFound(new ApiResponse<Payment_Method_Detail_DTO>
                (false, "Payment method updated, but details were not found"));

            return Ok(new ApiResponse<Payment_Method_Detail_DTO> (true, "", detailDto));
        }

        [HttpDelete("[controller]/PaymentMethod/{PaymentMethodID}")] // DELETE: /v1/payment/PaymentsMethods/5
        public async Task<ActionResult<ApiResponse<object>>> DeletePaymentMethod(Guid PaymentMethodID, CancellationToken cancellationToken)
        {
            var success = await _paymentService.DeletePaymentMethodAsync(PaymentMethodID, cancellationToken);
            if (!success)
            {
                return NotFound(new ApiResponse<object> (false, "Payment method not found"));
            }
            return Ok(new ApiResponse<object> (true, ""));
        }
    }
}
