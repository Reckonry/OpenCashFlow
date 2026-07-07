namespace OpenCashFlow.Application.Auth.Models;

public enum AuthFailure
{
    InvalidCredentials,
    NotActive,
    Locked,
    InternalError
}

public enum RegistrationFailure
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

public sealed class AuthenticatedUserResult
{
    public Guid UserID { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string UserFirstName { get; set; }
    public string? UserLastName { get; set; }
    public string? FullName { get; set; }
    public string? UserAvatar { get; set; }
    public string PasswordSalt { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool UserMustChangePassword { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public sealed record AuthClaimResult(string Type, string Value);

public sealed record LoginResult(
    bool Success,
    string? Token,
    bool RequiresPasswordChange,
    AuthFailure? ErrorType,
    Guid? UserID,
    Guid? TenantID,
    string? UserName);

public sealed record FastLoginResult(
    bool Success,
    string Message,
    string? Token,
    Guid? UserID,
    Guid? TenantID,
    string? UserName);

public sealed record FastLoginCookieResult(bool Success, string? FastLoginToken, AuthFailure? ErrorType);

public sealed record RegistrationResult(bool Success, RegistrationFailure ErrorType, string? ErrorMessage = null);

public sealed class RegistrationDraft
{
    public required string CompanyName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool AcceptPrivacyPolicy { get; set; }
}

public sealed record AccountConfirmationResult(bool Success);
