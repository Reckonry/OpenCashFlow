using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class Register_DTO
    {
        public required string CompanyName { get; set; } = null!;
        public required string Email { get; set; } = null!;
        public required string Password { get; set; } = null!;
        private string? _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword ?? Password;
            set => _confirmPassword = value;
        }

        public bool AcceptPrivacyPolicy { get; set; } = false;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
