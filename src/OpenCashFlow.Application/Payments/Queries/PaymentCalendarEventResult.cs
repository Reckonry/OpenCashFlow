namespace OpenCashFlow.Application.Payments.Queries;

public sealed record PaymentCalendarEventResult(
    string Id,
    string Title,
    DateTime Start,
    bool AllDay,
    string? Url,
    string Calendar,
    int PaymentCount,
    double TotalAmount,
    string Date,
    string? EntryType);
