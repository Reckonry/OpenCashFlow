using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using global::Shared.Models;
using System.Security.Claims;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public partial class PaymentController(IPaymentService PaymentService, ILogger<PaymentController> logger) : Controller
    {
        private readonly IPaymentService _paymentService = PaymentService;
        private readonly ILogger<PaymentController> _logger = logger;


        [HttpPost("[controller]s")] // GET /v1/payments?entryType=Exit&fromDate=2024-01-01&userId=...&page=1&pageSize=50
        public async Task<ActionResult<IEnumerable<Payment_List_DTO>?>> GetPayments([FromBody] Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
            // Get UserID and TenantID from claims if not specified in the filter
            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;

            // If IncludeAllUsers = true, do not force the UserID filter
            if (!filters.IncludeAllUsers && filters.UserID == null && Guid.TryParse(userIdClaim, out var userId))
                filters.UserID = userId;

            if (filters.TenantID == null && Guid.TryParse(companyIdClaim, out var companyId))
                filters.TenantID = companyId;

            var payments = await _paymentService.GetAllPaymentsAsync(filters, cancellationToken);
            return Ok(payments);
        }

        [HttpGet("[controller]/{PaymentID}")] // GET: Payments/5
        public async Task<ActionResult<Payment_Detail_DTO>> GetPayment(Guid PaymentID, CancellationToken cancellationToken)
        {
            var detail = await _paymentService.GetPaymentByIdAsync(PaymentID, cancellationToken);
            if (detail == null)
                return NotFound(new ApiResponse<Payment_Detail_DTO>
                    (false, $"Payment with ID '{PaymentID}' not found.", null));

            return Ok(new ApiResponse<Payment_Detail_DTO>(true, string.Empty, detail));
        }

        [HttpPost("[controller]")] // POST: Payments
        public async Task<ActionResult<ApiResponse<Payment_Detail_DTO>>> PostPayment([FromBody] Payment_Create_DTO payment, CancellationToken cancellationToken)
        {
            if (payment == null) return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, "Payment cannot be null"));

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, "Invalid payment data"));
            }

            try
            {
                Payment_Create_DTO? createdPayment = await _paymentService.AddPaymentAsync(payment, cancellationToken);
                if (createdPayment == null)
                    return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, "Failed to create payment"));

                var detailDto = await _paymentService.GetPaymentByIdAsync(createdPayment.PaymentID, cancellationToken);
                if (detailDto == null) return NotFound(new ApiResponse<Payment_Detail_DTO>(false, "Created payment not found"));

                var responseWrapper = new ApiResponse<Payment_Detail_DTO>(true, "", detailDto);
                return CreatedAtAction(nameof(GetPayment), new { PaymentID = detailDto.PaymentID }, responseWrapper);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating payment: {Message}", ex.Message);
                return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating payment");
                return StatusCode(500, new ApiResponse<Payment_Detail_DTO>(false, "An unexpected error occurred"));
            }
        }

        [HttpPut("[controller]/{PaymentID}")] // PUT: Payments/5
        public async Task<ActionResult<Payment_Detail_DTO>> PutPayment(Guid PaymentID, [FromBody] Payment_Detail_DTO payment, CancellationToken cancellationToken)
        {
            if (PaymentID != payment.PaymentID)
                return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, "ID mismatch"));

            var updated = await _paymentService.UpdatePaymentAsync(payment, cancellationToken);
            if (updated == null)
            {
                return NotFound(new ApiResponse<Payment_Detail_DTO>(false, "Payment not found"));
            }

            var detailDto = await _paymentService.GetPaymentByIdAsync(updated.PaymentID, cancellationToken);
            if (detailDto == null)
                return NotFound(new ApiResponse<Payment_Detail_DTO>
                (false, "Payment updated, but details were not found"));

            return Ok(new ApiResponse<Payment_Detail_DTO>(true, "", detailDto));
        }

        [HttpDelete("[controller]/{PaymentID}")] // DELETE: Payments/5
        public async Task<ActionResult<ApiResponse<object>>> DeletePayment(Guid PaymentID, CancellationToken cancellationToken)
        {
            var success = await _paymentService.DeletePaymentAsync(PaymentID, cancellationToken);
            if (!success)
            {
                return NotFound(new ApiResponse<object>(false, "Payment not found"));
            }
            return Ok(new ApiResponse<object>(true, ""));
        }

    }
}
