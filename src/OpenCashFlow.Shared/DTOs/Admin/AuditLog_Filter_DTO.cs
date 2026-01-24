using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Admin
{
    public class AuditLog_Filter_DTO
    {
        public string? EventType { get; set; }
        public string? Resource { get; set; }
        public Guid? UserID { get; set; }
        public Guid? TenantID { get; set; }
        public string? IPAddress { get; set; }
        public string? Severity { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public string? Search { get; set; } // Search in action, username, changes

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 50;

        public string SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }
}
