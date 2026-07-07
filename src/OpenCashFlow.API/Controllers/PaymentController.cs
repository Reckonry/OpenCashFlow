using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Contracts.Audit;
using OpenCashFlow.Contracts.Security;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public partial class PaymentController(IPaymentService PaymentService, IAuditLogService auditLogService, ILogger<PaymentController> logger) : Controller
    {
        private readonly IPaymentService _paymentService = PaymentService;
        private readonly IAuditLogService _auditLogService = auditLogService;
        private readonly ILogger<PaymentController> _logger = logger;


        [HttpPost("[controller]s")] // GET /v1/payments?entryType=Exit&fromDate=2024-01-01&userId=...&page=1&pageSize=50
        public async Task<ActionResult<IEnumerable<Payment_List_DTO>?>> GetPayments([FromBody] Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
            filters ??= new Payment_Filter_DTO();
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

        [HttpGet("[controller]/{PaymentID:guid}")] // GET: Payments/5
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

        [HttpPut("[controller]/{PaymentID:guid}")] // PUT: Payments/5
        public async Task<ActionResult<Payment_Detail_DTO>> PutPayment(Guid PaymentID, [FromBody] Payment_Detail_DTO payment, CancellationToken cancellationToken)
        {
            if (PaymentID != payment.PaymentID)
                return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, "ID mismatch"));

            Payment_Update_DTO? updated;
            try
            {
                updated = await _paymentService.UpdatePaymentAsync(payment, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<Payment_Detail_DTO>(false, ex.Message));
            }

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

        [HttpDelete("[controller]/{PaymentID:guid}")] // DELETE: Payments/5
        public async Task<ActionResult<ApiResponse<object>>> DeletePayment(Guid PaymentID, CancellationToken cancellationToken)
        {
            var success = await _paymentService.DeletePaymentAsync(PaymentID, cancellationToken);
            if (!success)
            {
                return NotFound(new ApiResponse<object>(false, "Payment not found"));
            }
            return Ok(new ApiResponse<object>(true, ""));
        }

        [HttpGet("[controller]s/export")]
        public async Task<IActionResult> ExportPayments([FromQuery] Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
            filters ??= new Payment_Filter_DTO();
            filters.IncludeAllUsers = true;
            filters.Page = 1;
            filters.PageSize = 200;

            if (Guid.TryParse(User.FindFirst("TenantID")?.Value, out var companyId))
            {
                filters.TenantID = companyId;
            }

            var payments = await _paymentService.GetAllPaymentsAsync(filters, cancellationToken) ?? Enumerable.Empty<Payment_List_DTO>();
            var csv = new StringBuilder();
            csv.AppendLine("PaymentID,Date,EntryType,Amount,PaymentMethod,DocumentType,Employee,Description");

            foreach (var payment in payments)
            {
                csv.AppendLine(string.Join(",",
                    Csv(payment.PaymentID.ToString()),
                    Csv(payment.DateIns.ToString("yyyy-MM-dd HH:mm:ss")),
                    Csv(payment.EntryType),
                    Csv(payment.Amount.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                    Csv(payment.PaymentMethodName),
                    Csv(payment.DocumentTypeName),
                    Csv(payment.EmployeeFullName),
                    Csv(payment.Description)));
            }

            await _auditLogService.LogEventAsync(
                AuditEventType.DataExported,
                "Payment",
                "ExportCsv",
                changes: new { filters.FromDate, filters.ToDate, filters.EntryType, filters.PaymentMethodID, filters.DocumentTypeID },
                cancellationToken: cancellationToken);

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"payments_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private static string Csv(string? value)
        {
            value ??= string.Empty;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

    }
}
