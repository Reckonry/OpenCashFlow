namespace OpenCashFlow.Application.Auth.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserID, string Token, string NewPassword);
