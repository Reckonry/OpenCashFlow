using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Contracts.Auth;

public class Core_Credentials
{
    [Required]
    public string? Username { get; set; }

    [Required, DataType(DataType.Password)]
    public string? Password { get; set; }

    public bool RememberMe { get; set; }
}

public class Core_FastLoginCredential
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "PIN")]
    public required string Pin { get; set; } = null!;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? FastLoginToken { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthErrorType? ErrorType { get; set; }
    public bool RequiresPasswordChange { get; set; }
}

public enum AuthErrorType
{
    InvalidCredentials,
    NotActive,
    Locked,
    InternalError
}

public class Core_RegistrationResult
{
    public bool Success { get; set; }
    public RegistrationError ErrorType { get; set; }
    public string? ErrorMessage { get; set; }

    public static Core_RegistrationResult Failure(RegistrationError error, string message)
    {
        return new Core_RegistrationResult { Success = false, ErrorMessage = message, ErrorType = error };
    }

    public static Core_RegistrationResult Failure(RegistrationError error)
    {
        return new Core_RegistrationResult { Success = false, ErrorType = error };
    }

    public static Core_RegistrationResult Ok()
    {
        return new Core_RegistrationResult { Success = true, ErrorType = RegistrationError.None };
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
