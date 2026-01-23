using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Cash;
using global::Shared.Models.Cash;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrator")]
    [Route("v{version:apiVersion}/admin/cash")]
    [ApiVersion("1.0")]
    public class CashAdminController(ICashService cashService, ILogger<CashAdminController> logger) : ControllerBase
    {
        private readonly ICashService _cash = cashService;
        private readonly ILogger<CashAdminController> _logger = logger;

        [HttpGet("current")]
        public async Task<ActionResult<decimal>> GetCurrent([FromQuery] Guid companyId, CancellationToken ct)
        {
            if (companyId == Guid.Empty) return BadRequest(Problem("companyId is required"));
            var bal = await _cash.GetCurrentAsync(companyId, ct);
            return Ok(bal);
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<IReadOnlyList<CashLedger>>> GetLedger([FromQuery] Guid companyId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        {
            if (companyId == Guid.Empty) return BadRequest(Problem("companyId is required"));
            take = Math.Clamp(take, 1, 200);
            var res = await _cash.GetLedgerAsync(companyId, from, to, skip, take, ct);
            return Ok(res);
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust([FromBody] CashAdjustRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(Problem("Reason is required"));

            // user id from claims for audit
            var userId = User?.FindFirst("UserID")?.Value ?? "unknown";
            await _cash.AdminAdjustAsync(request.CompanyId, request.Delta, request.Reason, userId, ct);
            return Ok();
        }

        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild([FromBody] CashRebuildRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            await _cash.RebuildBalanceAsync(request.CompanyId, ct);
            return Ok();
        }
    }
}

