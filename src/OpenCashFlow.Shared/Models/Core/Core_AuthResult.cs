using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Core
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? FastLoginToken { get; set; }
        public string? ErrorMessage { get; set; }
        public AuthErrorType? ErrorType { get; set; }
        public bool RequiresPasswordChange { get; set; } = false;
    }

    public enum AuthErrorType
    {
        InvalidCredentials,
        NotActive,
        Locked,
        InternalError
    }
}
