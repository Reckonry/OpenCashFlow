namespace OpenCashFlow.Application.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string? EmailOrUserName, Guid? UserID, string AppBaseUrl, int TokenTtlMinutes);
