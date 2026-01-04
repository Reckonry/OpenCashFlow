using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Admin;
using global::Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrator")]
    [Route("v{version:apiVersion}/Admin/AuditLog")]
    [ApiVersion("1.0")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// GET /v1/Admin/AuditLog - Lista audit log con filtri
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAuditLogs([FromQuery] AuditLog_Filter_DTO filters, CancellationToken cancellationToken)
        {
            try
            {
                filters ??= new AuditLog_Filter_DTO();
                var (logs, totalCount) = await _auditLogService.GetAuditLogsAsync(filters, cancellationToken);

                return Ok(new ApiResponse<object>(true, string.Empty, new
                {
                    logs,
                    totalCount,
                    page = filters.Page,
                    pageSize = filters.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize)
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, new ApiResponse<object>(false, "Errore nel recupero dei log", null));
            }
        }

        /// <summary>
        /// GET /v1/Admin/AuditLog/{id} - Dettaglio audit log
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AuditLog_Detail_DTO>>> GetAuditLog(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var log = await _auditLogService.GetAuditLogDetailAsync(id, cancellationToken);
                return Ok(new ApiResponse<AuditLog_Detail_DTO>(true, string.Empty, log));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Audit log {LogId} not found", id);
                return NotFound(new ApiResponse<AuditLog_Detail_DTO>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log {LogId}", id);
                return StatusCode(500, new ApiResponse<AuditLog_Detail_DTO>(false, "Errore nel recupero del log", null));
            }
        }

        /// <summary>
        /// GET /v1/Admin/AuditLog/Export - Esporta audit log in CSV
        /// </summary>
        [HttpGet("Export")]
        public async Task<IActionResult> ExportAuditLog([FromQuery] AuditLog_Filter_DTO filters, CancellationToken cancellationToken)
        {
            try
            {
                filters ??= new AuditLog_Filter_DTO();
                var csvBytes = await _auditLogService.ExportAuditLogAsync(filters, cancellationToken);

                var fileName = $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit log");
                return StatusCode(500, new ApiResponse<object>(false, "Errore nell'esportazione dei log", null));
            }
        }
    }
}
