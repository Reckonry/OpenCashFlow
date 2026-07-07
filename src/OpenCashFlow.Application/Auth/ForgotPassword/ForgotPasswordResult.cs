namespace OpenCashFlow.Application.Auth.ForgotPassword;

public sealed record ForgotPasswordResult(bool TokenCreated, Guid? UserID, string? UserName);
