using global::Shared.DTOs.Admin;

namespace OpenCashFlow.Admin.Models.AuditLog;

public class AuditLogDetailViewModel
{
    public AuditLog_Detail_DTO Log { get; set; } = new();
    public string? FormattedChanges { get; set; }
    public string? FormattedAdditionalInfo { get; set; }
}
