using System;

namespace Shared.DTOs.Admin
{
    public class User_List_DTO
    {
        public Guid UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string? CompanyName { get; set; }
        public Guid? TenantID { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
