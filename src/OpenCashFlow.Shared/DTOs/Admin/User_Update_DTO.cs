using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Admin
{
    public class User_Update_DTO
    {
        [Required]
        public Guid UserID { get; set; }

        [EmailAddress(ErrorMessage = "Email non valida")]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Numero di telefono non valido")]
        public string? PhoneNumber { get; set; }

        public bool? IsActive { get; set; }

        public Guid? TenantID { get; set; }
    }
}
