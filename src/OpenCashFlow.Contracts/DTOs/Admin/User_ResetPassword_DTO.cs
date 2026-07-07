using System;
using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Contracts.DTOs.Admin
{
    public class User_ResetPassword_DTO
    {
        [Required]
        public Guid UserID { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must include at least one uppercase, one lowercase, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;

        public bool RequirePasswordChange { get; set; } = true;
        public bool SendNotificationEmail { get; set; } = true;
    }
}
