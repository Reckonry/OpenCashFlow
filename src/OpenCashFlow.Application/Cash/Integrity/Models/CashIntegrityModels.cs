using OpenCashFlow.Domain.Cash;

namespace OpenCashFlow.Application.Cash.Integrity.Models;

public sealed record CashAccountResult(
    Guid CashAccountId,
    Guid TenantId,
    string Name,
    CashAccountType Type,
    string Currency,
    bool IsDefault,
    bool IsActive);

public sealed record CashAccountBalanceResult(
    Guid TenantId,
    Guid CashAccountId,
    decimal ExpectedBalance,
    string Currency,
    DateTimeOffset CalculatedAtUtc);

public sealed record CashMovementResult(
    Guid CashMovementId,
    Guid TenantId,
    Guid CashAccountId,
    decimal Amount,
    string Currency,
    CashMovementDirection Direction,
    string ReasonCategory,
    string? ReasonText,
    Guid? CashHandlerUserId,
    Guid RecordedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset? PostedAtUtc,
    CashMovementStatus Status,
    bool PhysicalCashHandled,
    Guid? OriginalMovementId,
    Guid? CorrectionGroupId,
    Guid? CashTransferId,
    CashTransferSide? TransferSide);

public sealed record CashTransferResult(
    Guid CashTransferId,
    Guid TenantId,
    Guid SourceCashAccountId,
    Guid DestinationCashAccountId,
    decimal Amount,
    string Currency,
    string ReasonCategory,
    string? ReasonText,
    Guid RecordedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset? PostedAtUtc,
    CashTransferStatus Status,
    Guid? OriginalTransferId,
    Guid? CorrectionGroupId,
    CashMovementResult? SourceMovement,
    CashMovementResult? DestinationMovement);

public sealed record CashTransferCorrectionResult(
    CashTransferResult Original,
    CashTransferResult Reversal,
    CashTransferResult Replacement);

public sealed record CashSessionResult(
    Guid CashSessionId,
    Guid TenantId,
    Guid CashAccountId,
    DateOnly BusinessDate,
    decimal OpeningExpectedBalance,
    decimal? OpeningActualBalance,
    string Currency,
    Guid OpenedByUserId,
    DateTimeOffset OpenedAtUtc,
    CashSessionStatus Status);

public sealed record CashReconciliationResult(
    Guid CashReconciliationId,
    Guid TenantId,
    Guid CashSessionId,
    Guid CashAccountId,
    decimal ExpectedBalance,
    decimal ActualBalance,
    decimal Discrepancy,
    string Currency,
    CashReconciliationStatus Status,
    Guid ReconciledByUserId,
    DateTimeOffset ReconciledAtUtc);

public sealed record CashDiscrepancyResult(
    Guid CashDiscrepancyId,
    Guid TenantId,
    Guid CashReconciliationId,
    decimal Amount,
    string Currency,
    string Category,
    string Explanation,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAtUtc);

public sealed record CashIntegrityAuditEvent(
    Guid TenantId,
    string EventType,
    string Resource,
    Guid ResourceId,
    Guid ActorUserId,
    DateTimeOffset OccurredAtUtc,
    string? Details);
