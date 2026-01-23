using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Admin
{
    public class User_Filter_DTO
    {
        public string? Search { get; set; }
        public Guid? TenantID { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? LastLoginFrom { get; set; }
        public DateTime? LastLoginTo { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 50;

        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
    }
}
