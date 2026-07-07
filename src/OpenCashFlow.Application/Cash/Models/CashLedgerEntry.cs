namespace OpenCashFlow.Application.Cash.Models;

public sealed record CashLedgerEntry(
    Guid Id,
    Guid CompanyId,
    string RefType,
    Guid RefId,
    Guid? OriginalPaymentId,
    decimal Delta,
    string? Reason,
    string CreatedBy,
    DateTimeOffset CreatedAtUtc);
