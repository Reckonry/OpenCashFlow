using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Core
{
    public class Core_RegistrationResult
    {
        public bool Success { get; set; }
        public RegistrationError ErrorType { get; set; }
        public string? ErrorMessage { get; set; }

        public static Core_RegistrationResult Failure(RegistrationError error, string v)
        {
            return new Core_RegistrationResult() { Success = false, ErrorMessage = v, ErrorType = error };
        }

        public static Core_RegistrationResult Failure(RegistrationError error)
        {
            return new Core_RegistrationResult() { Success = false, ErrorType = error };
        }

        public static Core_RegistrationResult Ok()
        {
            return new Core_RegistrationResult() { Success = true, ErrorType = RegistrationError.None };
        }

    }

    public enum RegistrationError
    {
        None,
        UsernameTaken,
        EmailTaken,
        WeakPassword,
        InvalidEmail,
        UnknownError,
        MissingRequiredFields,
        PasswordsDoNotMatch,
        PrivacyPolicyNotAccepted
    }
}