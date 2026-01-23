using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Employees
{
    public class Employee_MyProfile_Update_DTO
    {
        [Required]
        public string UserFirstName { get; set; } = string.Empty;
        public string? UserLastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumberPrefix { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public string Language { get; set; } = "it";
        [Required]
        public string Country { get; set; } = "IT";
        public string? Timezone { get; set; }
    }
}

