namespace OpenCashFlow.Application.Employees.ResendPin;

public sealed record ResendEmployeePinCommand(Guid TenantID, Guid UserID);
