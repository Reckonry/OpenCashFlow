using OpenCashFlow.Admin.Models.AuditLog;
using OpenCashFlow.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Admin;
using global::Shared.Enums;
using System.Text.Json;

namespace OpenCashFlow.Admin.Controllers;

[Authorize(Policy = "GIManagers")]
public class AuditLogController : Controller
{
    private readonly AuditLogAPIService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(AuditLogAPIService auditLogService, ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AuditLog_Filter_DTO? filters)
    {
        filters ??= new AuditLog_Filter_DTO();
        filters.Page = filters.Page <= 0 ? 1 : filters.Page;
        filters.PageSize = filters.PageSize <= 0 ? 50 : filters.PageSize;

        var response = await _auditLogService.GetAuditLogsAsync(filters, HttpContext.RequestAborted);

        var viewModel = new AuditLogIndexViewModel
        {
            Filters = filters,
            EventTypes = Enum.GetNames(typeof(AuditEventType)).OrderBy(t => t).ToList(),
            Severities = new[] { "Info", "Warning", "Error", "Critical" }
        };

        if (!response.Success || response.Data is null)
        {
            TempData["Error"] = response.Message ?? "Unable to retrieve logs.";
            return View(viewModel);
        }

        try
        {
            var jsonElement = (JsonElement)response.Data;
            var logsJson = jsonElement.GetProperty("logs").GetRawText();
            viewModel.Logs = JsonSerializer.Deserialize<List<AuditLog_List_DTO>>(logsJson) ?? [];
            viewModel.TotalCount = jsonElement.GetProperty("totalCount").GetInt32();
            viewModel.TotalPages = jsonElement.GetProperty("totalPages").GetInt32();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel parsing della risposta dell'audit log.");
            TempData["Error"] = "Invalid response from the audit log service.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var response = await _auditLogService.GetAuditLogAsync(id, HttpContext.RequestAborted);

        if (!response.Success || response.Data is null)
        {
            TempData["Error"] = response.Message ?? "Log not found.";
            return RedirectToAction(nameof(Index));
        }

        var detailViewModel = new AuditLogDetailViewModel
        {
            Log = response.Data,
            FormattedChanges = FormatJsonIfPossible(response.Data.Changes),
            FormattedAdditionalInfo = FormatJsonIfPossible(response.Data.AdditionalInfo) ?? response.Data.AdditionalInfo
        };

        return View(detailViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] AuditLog_Filter_DTO? filters)
    {
        filters ??= new AuditLog_Filter_DTO();

        var exportResult = await _auditLogService.ExportAuditLogAsync(filters, HttpContext.RequestAborted);

        if (!exportResult.Success || exportResult.Content == null)
        {
            TempData["Error"] = exportResult.Message ?? "Unable to export the audit log.";
            return RedirectToAction(nameof(Index), filters);
        }

        var fileName = string.IsNullOrWhiteSpace(exportResult.FileName)
            ? $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv"
            : exportResult.FileName!;

        return File(exportResult.Content, "text/csv", fileName);
    }

    private static string? FormatJsonIfPossible(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return json;
        }
    }
}
