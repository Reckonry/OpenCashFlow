using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Core
{
    public class Core_Credentials
    {
        [Required]
        public string? Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class Core_FastLoginCredential
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "PIN")]
        public required string Pin { get; set; } = null!;
    }
}
