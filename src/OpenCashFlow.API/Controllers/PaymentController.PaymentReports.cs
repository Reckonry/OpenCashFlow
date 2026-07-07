using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.API.Controllers
{
    public partial class PaymentController : Controller
    {
        [HttpGet("[controller]/daily-payments")] // GET /v1/Payment/daily-payments?date=2024-01-01
        public async Task<ActionResult<double>> GetDailyPayments([FromQuery] DateTime date, CancellationToken cancellationToken)
        {
            if (date == default)
                return BadRequest(new ApiResponse<double>(false, "Date parameter is required."));

            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;
            if (companyIdClaim == null || !Guid.TryParse(companyIdClaim, out var TenantID))
                return BadRequest(new ApiResponse<double>(false, "Invalid or missing TenantID."));
            var report = await _paymentService.GetDailyPaymentsAsync(TenantID, date, cancellationToken);
            return Ok(new ApiResponse<double>(true, string.Empty, report));
        }

        [HttpGet("[controller]/monthly-payments")] // GET /v1/Payment/monthly-payments?year=2024&month=1
        public async Task<ActionResult<double>> GetMonthlyPayments([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
        {
            if (year <= 0 || month <= 0 || month > 12)
                return BadRequest(new ApiResponse<double>(false, "Valid year and month parameters are required."));
            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;
            if (companyIdClaim == null || !Guid.TryParse(companyIdClaim, out var TenantID))
                return BadRequest(new ApiResponse<double>(false, "Invalid or missing TenantID."));
            var report = await _paymentService.GetMonthlyPaymentsAsync(TenantID, year, month, cancellationToken);
            return Ok(new ApiResponse<double>(true, string.Empty, report));
        }

        [HttpGet("[controller]/yearly-payments")] // GET /v1/Payment/yearly-payments?year=2024
        public async Task<ActionResult<double>> GetYearlyPayments([FromQuery] int year, CancellationToken cancellationToken)
        {
            if (year <= 0)
                return BadRequest(new ApiResponse<double>(false, "Valid year parameter is required."));
            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;
            if (companyIdClaim == null || !Guid.TryParse(companyIdClaim, out var TenantID))
                return BadRequest(new ApiResponse<double>(false, "Invalid or missing TenantID."));
            var report = await _paymentService.GetYearlyPaymentsAsync(TenantID, year, cancellationToken);
            return Ok(new ApiResponse<double>(true, string.Empty, report));
        }

        [HttpGet("[controller]/payments-in-period")] // GET /v1/Payment/payments-in-period?startDate=2024-01-01&endDate=2024-01-31
        public async Task<ActionResult<double>> GetPaymentsInPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            if (startDate == default || endDate == default || startDate > endDate)
                return BadRequest(new ApiResponse<double>(false, "Valid startDate and endDate parameters are required."));
            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;
            if (companyIdClaim == null || !Guid.TryParse(companyIdClaim, out var TenantID))
                return BadRequest(new ApiResponse<double>(false, "Invalid or missing TenantID."));
            var report = await _paymentService.GetTotalPaymentsInPeriodAsync(TenantID, startDate, endDate, cancellationToken);
            return Ok(new ApiResponse<double>(true, string.Empty, report));
        }

        [HttpGet("[controller]/detailed-payments-in-period")] // GET /v1/Payment/detailed-payments-in-period?startDate=2024-01-01&endDate=2024-01-31
        public async Task<ActionResult<IEnumerable<Payment_DailyPayments>>> GetDetailedPaymentsInPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            if (startDate == default || endDate == default || startDate > endDate)
                return BadRequest(new ApiResponse<IEnumerable<Payment_DailyPayments>>(false, "Valid startDate and endDate parameters are required.", null));
            var userIdClaim = User.FindFirst("UserID")?.Value;
            var companyIdClaim = User.FindFirst("TenantID")?.Value;
            if (companyIdClaim == null || !Guid.TryParse(companyIdClaim, out var TenantID))
                return BadRequest(new ApiResponse<IEnumerable<Payment_DailyPayments>>(false, "Invalid or missing TenantID.", null));
            var report = await _paymentService.GetDailyPaymentsInPeriodAsync(TenantID, startDate, endDate, cancellationToken);
            return Ok(new ApiResponse<IEnumerable<Payment_DailyPayments>>(true, string.Empty, report));
        }
        
    }
}
