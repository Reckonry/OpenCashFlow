using System;

namespace OpenCashFlow.Contracts.DTOs.Admin
{
    public class AuditLog_List_DTO
    {
        public Guid AuditLogID { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? ResourceID { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid? UserID { get; set; }
        public string? Username { get; set; }
        public string? IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Severity { get; set; }
        public Guid? TenantID { get; set; }
    }
}
