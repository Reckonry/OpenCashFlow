using OpenCashFlow.Contracts.DTOs.Admin;

namespace OpenCashFlow.Admin.Models.AuditLog;

public class AuditLogIndexViewModel
{
    public List<AuditLog_List_DTO> Logs { get; set; } = [];
    public AuditLog_Filter_DTO Filters { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IReadOnlyList<string> EventTypes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Severities { get; set; } = Array.Empty<string>();
    public int CurrentPage => Filters.Page;
}
