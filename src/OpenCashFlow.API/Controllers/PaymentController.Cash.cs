using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.Audit;
using OpenCashFlow.Contracts.Security;
using OpenCashFlow.Contracts.Cash;
using OpenCashFlow.Contracts.DTOs.Cash;
using System.Text;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "CompanyAdmin,InstanceAdmin")]
    [Route("v{version:apiVersion}/admin/cash")]
    [ApiVersion("1.0")]
    public class CashAdminController(ICashService cashService, IAuditLogService auditLogService, ILogger<CashAdminController> logger) : ControllerBase
    {
        private readonly ICashService _cash = cashService;
        private readonly IAuditLogService _auditLogService = auditLogService;
        private readonly ILogger<CashAdminController> _logger = logger;

        [HttpGet("current")]
        public async Task<ActionResult<decimal>> GetCurrent([FromQuery] Guid companyId, CancellationToken ct)
        {
            if (!TryResolveCompanyId(companyId, out var resolvedCompanyId, out var error)) return error!;
            var bal = await _cash.GetCurrentAsync(resolvedCompanyId, ct);
            return Ok(bal);
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<IReadOnlyList<CashLedger>>> GetLedger([FromQuery] Guid companyId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
        {
            if (!TryResolveCompanyId(companyId, out var resolvedCompanyId, out var error)) return error!;
            skip = Math.Max(skip, 0);
            take = Math.Clamp(take, 1, 200);
            var res = await _cash.GetLedgerAsync(resolvedCompanyId, from, to, skip, take, ct);
            return Ok(res);
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust([FromBody] CashAdjustRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!TryResolveCompanyId(request.CompanyId, out var resolvedCompanyId, out var error)) return error!;
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(Problem("Reason is required"));
            if (request.Delta == 0m)
                return BadRequest(Problem("Delta must be different from zero"));

            // user id from claims for audit
            var userId = User?.FindFirst("UserID")?.Value ?? "unknown";
            await _cash.AdminAdjustAsync(resolvedCompanyId, request.Delta, request.Reason, userId, ct);
            await _auditLogService.LogEventAsync(
                AuditEventType.ConfigurationChanged,
                "CashLedger",
                "ManualAdjustment",
                changes: new { CompanyId = resolvedCompanyId, request.Delta, request.Reason },
                cancellationToken: ct);
            return Ok();
        }

        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild([FromBody] CashRebuildRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!TryResolveCompanyId(request.CompanyId, out var resolvedCompanyId, out var error)) return error!;
            await _cash.RebuildBalanceAsync(resolvedCompanyId, ct);
            await _auditLogService.LogEventAsync(
                AuditEventType.ConfigurationChanged,
                "CashBalance",
                "Rebuild",
                changes: new { CompanyId = resolvedCompanyId },
                cancellationToken: ct);
            return Ok();
        }

        [HttpGet("ledger/export")]
        public async Task<IActionResult> ExportLedger([FromQuery] Guid companyId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        {
            if (!TryResolveCompanyId(companyId, out var resolvedCompanyId, out var error)) return error!;
            var rows = await _cash.GetLedgerAsync(resolvedCompanyId, from, to, 0, 200, ct);
            var csv = new StringBuilder();
            csv.AppendLine("Id,CreatedAtUtc,RefType,RefId,OriginalPaymentId,Delta,CreatedBy,Reason");

            foreach (var row in rows)
            {
                csv.AppendLine(string.Join(",",
                    Csv(row.Id.ToString()),
                    Csv(row.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss")),
                    Csv(row.RefType),
                    Csv(row.RefId.ToString()),
                    Csv(row.OriginalPaymentId?.ToString()),
                    Csv(row.Delta.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)),
                    Csv(row.CreatedBy),
                    Csv(row.Reason)));
            }

            await _auditLogService.LogEventAsync(
                AuditEventType.DataExported,
                "CashLedger",
                "ExportCsv",
                changes: new { CompanyId = resolvedCompanyId, from, to },
                cancellationToken: ct);

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"cash_ledger_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private bool TryResolveCompanyId(Guid requestedCompanyId, out Guid companyId, out ActionResult? error)
        {
            companyId = Guid.Empty;
            error = null;

            if (User.IsInRole("InstanceAdmin"))
            {
                if (requestedCompanyId == Guid.Empty)
                {
                    error = BadRequest(Problem("companyId is required"));
                    return false;
                }

                companyId = requestedCompanyId;
                return true;
            }

            if (!Guid.TryParse(User.FindFirst("TenantID")?.Value, out var tenantId) || tenantId == Guid.Empty)
            {
                error = Unauthorized(Problem("Tenant claim is missing"));
                return false;
            }

            if (requestedCompanyId != Guid.Empty && requestedCompanyId != tenantId)
            {
                error = Forbid();
                return false;
            }

            companyId = tenantId;
            return true;
        }

        private static string Csv(string? value)
        {
            value ??= string.Empty;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
