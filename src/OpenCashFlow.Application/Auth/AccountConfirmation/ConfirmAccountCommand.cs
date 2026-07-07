namespace OpenCashFlow.Application.Auth.AccountConfirmation;

public sealed record ConfirmAccountCommand(Guid TenantID, Guid UserID);
