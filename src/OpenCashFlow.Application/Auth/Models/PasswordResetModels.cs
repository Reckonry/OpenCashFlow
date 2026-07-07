namespace OpenCashFlow.Application.Auth.Models;

public sealed record AuthUserCredential(
    Guid UserID,
    string UserName,
    string Email,
    string UserFirstName,
    string PasswordSalt,
    bool IsApproved,
    bool LockoutEnabled);

public sealed record PasswordResetTokenValidation(
    bool IsValid,
    bool IsExpired,
    Guid? UserID,
    string? UserName,
    string? Email,
    string? UserFirstName);
