namespace OpenCashFlow.Application.Auth.ResetPassword;

public sealed record ValidateResetTokenResult(bool IsValid, bool IsExpired, Guid? UserID);
