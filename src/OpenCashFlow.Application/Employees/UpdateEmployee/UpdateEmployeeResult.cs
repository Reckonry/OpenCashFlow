namespace OpenCashFlow.Application.Employees.UpdateEmployee;

public sealed record UpdateEmployeeResult(
    bool Success,
    bool EmailChanged,
    bool PinSentSuccessfully,
    string Message,
    string? NewEmail);
