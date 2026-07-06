using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class SetupStatus_DTO
    {
        public bool RequiresSetup { get; set; }
        public bool HasCompanies { get; set; }
        public bool HasAdminUsers { get; set; }
    }

    public class SetupRequest_DTO
    {
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public required string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public required string AdminEmail { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public required string AdminPassword { get; set; }

        [Compare(nameof(AdminPassword))]
        public required string ConfirmPassword { get; set; }

        [Required]
        [StringLength(80)]
        public required string AdminFirstName { get; set; }

        [StringLength(80)]
        public string? AdminLastName { get; set; }

        [Required]
        [StringLength(5)]
        public string Language { get; set; } = "it";

        [Required]
        [StringLength(16)]
        public string Currency { get; set; } = "EUR";

        [Required]
        [StringLength(80)]
        public string Timezone { get; set; } = "Europe/Rome";

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string Country { get; set; } = "IT";
    }
}
