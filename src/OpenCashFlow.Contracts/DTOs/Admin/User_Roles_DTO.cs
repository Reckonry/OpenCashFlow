using System;
using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Contracts.DTOs.Admin
{
    public class User_Roles_DTO
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public string[] Roles { get; set; } = Array.Empty<string>();
    }

    public class Role_DTO
    {
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
