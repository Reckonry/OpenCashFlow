using System;
using System.Collections.Generic;

namespace Shared.DTOs.Admin
{
    public class User_Detail_DTO
    {
        public Guid UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public Guid? TenantID { get; set; }
        public string? CompanyName { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        // Audit info
        public List<UserAuditEntry_DTO> AuditLog { get; set; } = new();
    }

    public class UserAuditEntry_DTO
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? PerformedBy { get; set; }
        public string? IPAddress { get; set; }
    }
}
