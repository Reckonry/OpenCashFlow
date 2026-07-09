namespace OpenCashFlow.Application.Cash.Integrity.Commands;

public sealed record CreateCashMovementCommand(
    Guid TenantId,
    Guid CashAccountId,
    decimal Amount,
    string Currency,
    string Direction,
    string ReasonCategory,
    string? ReasonText,
    Guid? CashHandlerUserId,
    Guid RecordedByUserId,
    DateTimeOffset OccurredAtUtc,
    bool PhysicalCashHandled);

public sealed record ReverseCashMovementCommand(
    Guid TenantId,
    Guid CashMovementId,
    Guid RecordedByUserId,
    string ReasonCategory,
    string ReasonText,
    DateTimeOffset OccurredAtUtc);

public sealed record CorrectCashMovementCommand(
    Guid TenantId,
    Guid CashMovementId,
    decimal ReplacementAmount,
    string Currency,
    string ReplacementDirection,
    string ReasonCategory,
    string ReasonText,
    Guid RecordedByUserId,
    DateTimeOffset OccurredAtUtc);

public sealed record CreateCashTransferCommand(
    Guid TenantId,
    Guid SourceCashAccountId,
    Guid DestinationCashAccountId,
    decimal Amount,
    string Currency,
    string ReasonCategory,
    string? ReasonText,
    Guid RecordedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset OccurredAtUtc);

public sealed record ReverseCashTransferCommand(
    Guid TenantId,
    Guid CashTransferId,
    Guid RecordedByUserId,
    string ReasonCategory,
    string ReasonText,
    DateTimeOffset OccurredAtUtc);

public sealed record CorrectCashTransferCommand(
    Guid TenantId,
    Guid CashTransferId,
    Guid ReplacementSourceCashAccountId,
    Guid ReplacementDestinationCashAccountId,
    decimal ReplacementAmount,
    string Currency,
    string ReasonCategory,
    string ReasonText,
    Guid RecordedByUserId,
    DateTimeOffset OccurredAtUtc);

public sealed record OpenCashSessionCommand(
    Guid TenantId,
    Guid CashAccountId,
    DateOnly BusinessDate,
    decimal OpeningExpectedBalance,
    decimal? OpeningActualBalance,
    string Currency,
    Guid OpenedByUserId,
    DateTimeOffset OpenedAtUtc);

public sealed record ReconcileCashSessionCommand(
    Guid TenantId,
    Guid CashSessionId,
    Guid CashAccountId,
    decimal ExpectedBalance,
    decimal ActualBalance,
    string Currency,
    Guid ReconciledByUserId,
    DateTimeOffset ReconciledAtUtc);

public sealed record ExplainCashDiscrepancyCommand(
    Guid TenantId,
    Guid CashReconciliationId,
    Guid CashDiscrepancyId,
    decimal Amount,
    string Currency,
    string Category,
    string Explanation,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc);
