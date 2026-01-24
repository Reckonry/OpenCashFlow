using global::Shared.DTOs.Admin;
using global::Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OpenCashFlow.Admin.Services;

public class AuditLogAPIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditLogAPIService> _logger;

    public AuditLogAPIService(IHttpClientFactory httpClientFactory, ILogger<AuditLogAPIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("API-Client");
        _logger = logger;
    }

    public async Task<ApiResponse<object>> GetAuditLogsAsync(AuditLog_Filter_DTO filters, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = BuildQueryString(filters);
            var response = await _httpClient.GetAsync($"/v1/Admin/AuditLog?{queryParams}", cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError("Errore HTTP GET /v1/Admin/AuditLog: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<object>(false, $"Errore durante il recupero dell'audit log ({response.StatusCode}).", null);
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return payload ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dell'audit log.");
            return new ApiResponse<object>(false, "Errore nel recupero dell'audit log.", null);
        }
    }

    public async Task<ApiResponse<AuditLog_Detail_DTO>> GetAuditLogAsync(Guid auditLogId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/Admin/AuditLog/{auditLogId}", cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ApiResponse<AuditLog_Detail_DTO>(false, "Audit log non trovato.", null);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError("Errore HTTP GET /v1/Admin/AuditLog/{AuditLogId}: {Status} - {Error}", auditLogId, response.StatusCode, errorText);
                return new ApiResponse<AuditLog_Detail_DTO>(false, $"Errore durante il recupero ({response.StatusCode}).", null);
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuditLog_Detail_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return payload ?? new ApiResponse<AuditLog_Detail_DTO>(false, "Risposta non valida dal server.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dell'audit log {AuditLogId}", auditLogId);
            return new ApiResponse<AuditLog_Detail_DTO>(false, "Errore nel recupero dell'audit log.", null);
        }
    }

    public async Task<AuditLogExportResult> ExportAuditLogAsync(AuditLog_Filter_DTO filters, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = BuildQueryString(filters);
            var response = await _httpClient.GetAsync($"/v1/Admin/AuditLog/Export?{queryParams}", cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogError("Errore HTTP GET /v1/Admin/AuditLog/Export: {Status} - {Error}", response.StatusCode, errorText);
                return new AuditLogExportResult(false, $"Errore durante l'esportazione ({response.StatusCode}).", null, null);
            }

            var contentBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var fileName = ResolveFileName(response.Content.Headers.ContentDisposition);

            return new AuditLogExportResult(true, string.Empty, contentBytes, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'esportazione dell'audit log.");
            return new AuditLogExportResult(false, "Errore durante l'esportazione dell'audit log.", null, null);
        }
    }

    private static string BuildQueryString(AuditLog_Filter_DTO filters)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(filters.EventType))
            queryParams.Add($"EventType={Uri.EscapeDataString(filters.EventType)}");

        if (!string.IsNullOrWhiteSpace(filters.Resource))
            queryParams.Add($"Resource={Uri.EscapeDataString(filters.Resource)}");

        if (!string.IsNullOrWhiteSpace(filters.Search))
            queryParams.Add($"Search={Uri.EscapeDataString(filters.Search)}");

        if (!string.IsNullOrWhiteSpace(filters.IPAddress))
            queryParams.Add($"IPAddress={Uri.EscapeDataString(filters.IPAddress)}");

        if (!string.IsNullOrWhiteSpace(filters.Severity))
            queryParams.Add($"Severity={Uri.EscapeDataString(filters.Severity)}");

        if (filters.UserID.HasValue)
            queryParams.Add($"UserID={filters.UserID.Value}");

        if (filters.TenantID.HasValue)
            queryParams.Add($"TenantID={filters.TenantID.Value}");

        if (filters.TimestampFrom.HasValue)
            queryParams.Add($"TimestampFrom={filters.TimestampFrom.Value:O}");

        if (filters.TimestampTo.HasValue)
            queryParams.Add($"TimestampTo={filters.TimestampTo.Value:O}");

        queryParams.Add($"Page={filters.Page}");
        queryParams.Add($"PageSize={filters.PageSize}");
        queryParams.Add($"SortBy={Uri.EscapeDataString(filters.SortBy)}");
        queryParams.Add($"SortDescending={filters.SortDescending}");

        return string.Join("&", queryParams);
    }

    private static string ResolveFileName(ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition == null)
        {
            return $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        }

        var fileName =
            contentDisposition.FileNameStar ??
            contentDisposition.FileName?
                .Trim('"')
                .Trim();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        }

        return fileName;
    }
}

public record AuditLogExportResult(bool Success, string? Message, byte[]? Content, string? FileName);
