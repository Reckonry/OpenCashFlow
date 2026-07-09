using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Contracts.DTOs
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

        // Kept optional for older clients. First-run setup now generates the temporary password server-side.
        public string? AdminPassword { get; set; }

        // Kept optional for older clients. First-run setup now generates the temporary password server-side.
        public string? ConfirmPassword { get; set; }

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

    public class SetupCompleted_DTO
    {
        public required SetupStatus_DTO Status { get; set; }
        public required string AdminEmail { get; set; }
        public required string TemporaryAdminPassword { get; set; }
        public string Message { get; set; } = "Setup completed. Store the temporary password now; it is shown only once.";
    }
}
