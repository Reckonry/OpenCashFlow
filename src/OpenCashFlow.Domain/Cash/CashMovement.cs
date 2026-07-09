using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashMovement(
    Guid CashMovementId,
    TenantId TenantId,
    Guid CashAccountId,
    Money Amount,
    CashMovementDirection Direction,
    string ReasonCategory,
    string? ReasonText,
    UserId? CashHandlerUserId,
    UserId RecordedByUserId,
    UserId? ApprovedByUserId,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset? PostedAtUtc,
    CashMovementStatus Status,
    bool PhysicalCashHandled,
    Guid? OriginalMovementId,
    Guid? CorrectionGroupId,
    Guid? CashTransferId,
    CashTransferSide? TransferSide)
{
    public static CashMovement CreateDraft(
        Guid cashMovementId,
        TenantId tenantId,
        Guid cashAccountId,
        Money amount,
        CashMovementDirection direction,
        string reasonCategory,
        string? reasonText,
        UserId? cashHandlerUserId,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc,
        bool physicalCashHandled)
    {
        ValidateCore(cashMovementId, cashAccountId, amount, reasonCategory, cashHandlerUserId, physicalCashHandled);

        return new CashMovement(
            cashMovementId,
            tenantId,
            cashAccountId,
            amount,
            direction,
            reasonCategory.Trim(),
            NormalizeOptional(reasonText),
            cashHandlerUserId,
            recordedByUserId,
            ApprovedByUserId: null,
            occurredAtUtc,
            PostedAtUtc: null,
            CashMovementStatus.Draft,
            physicalCashHandled,
            OriginalMovementId: null,
            CorrectionGroupId: null,
            CashTransferId: null,
            TransferSide: null);
    }

    public static CashMovement CreatePostedTransferSide(
        Guid cashMovementId,
        TenantId tenantId,
        Guid cashAccountId,
        Money amount,
        CashMovementDirection direction,
        Guid cashTransferId,
        CashTransferSide transferSide,
        string reasonCategory,
        string? reasonText,
        UserId recordedByUserId,
        UserId? approvedByUserId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc)
    {
        if (cashTransferId == Guid.Empty)
        {
            throw new ArgumentException("Cash transfer id is required.", nameof(cashTransferId));
        }

        return CreateDraft(
            cashMovementId,
            tenantId,
            cashAccountId,
            amount,
            direction,
            reasonCategory,
            reasonText,
            cashHandlerUserId: null,
            recordedByUserId,
            occurredAtUtc,
            physicalCashHandled: false).Post(postedAtUtc, approvedByUserId) with
        {
            CashTransferId = cashTransferId,
            TransferSide = transferSide
        };
    }

    public CashMovement Post(DateTimeOffset postedAtUtc, UserId? approvedByUserId = null)
    {
        if (Status != CashMovementStatus.Draft)
        {
            throw new InvalidOperationException("Only draft cash movements can be posted.");
        }

        return this with
        {
            Status = CashMovementStatus.Posted,
            PostedAtUtc = postedAtUtc,
            ApprovedByUserId = approvedByUserId
        };
    }

    public CashMovement UpdateDraftReason(string reasonCategory, string? reasonText)
    {
        if (Status != CashMovementStatus.Draft)
        {
            throw new InvalidOperationException("Posted cash movements cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(reasonCategory))
        {
            throw new ArgumentException("Cash movement reason/category is required.", nameof(reasonCategory));
        }

        return this with
        {
            ReasonCategory = reasonCategory.Trim(),
            ReasonText = NormalizeOptional(reasonText)
        };
    }

    public CashMovement CreatePostedReversal(
        Guid reversalMovementId,
        UserId recordedByUserId,
        string reasonCategory,
        string reasonText,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc)
    {
        if (Status != CashMovementStatus.Posted)
        {
            throw new InvalidOperationException("Only posted cash movements can be reversed.");
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            throw new ArgumentException("Reversal reason is required.", nameof(reasonText));
        }

        var reversal = CreateDraft(
            reversalMovementId,
            TenantId,
            CashAccountId,
            Amount,
            Reverse(Direction),
            reasonCategory,
            reasonText,
            CashHandlerUserId,
            recordedByUserId,
            occurredAtUtc,
            PhysicalCashHandled);

        return reversal.Post(postedAtUtc) with
        {
            OriginalMovementId = CashMovementId,
            CorrectionGroupId = CorrectionGroupId ?? Guid.NewGuid(),
            CashTransferId = CashTransferId,
            TransferSide = TransferSide
        };
    }

    public CashMovementCorrection CreateCorrection(
        Guid reversalMovementId,
        Guid replacementMovementId,
        Money replacementAmount,
        CashMovementDirection replacementDirection,
        string reasonCategory,
        string reasonText,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc)
    {
        if (Status != CashMovementStatus.Posted)
        {
            throw new InvalidOperationException("Only posted cash movements can be corrected.");
        }

        var correctionGroupId = Guid.NewGuid();
        var reversal = CreatePostedReversal(
            reversalMovementId,
            recordedByUserId,
            reasonCategory,
            reasonText,
            occurredAtUtc,
            postedAtUtc) with
        {
            CorrectionGroupId = correctionGroupId
        };

        var original = this with
        {
            Status = CashMovementStatus.Corrected,
            CorrectionGroupId = correctionGroupId
        };

        var replacement = CreateDraft(
            replacementMovementId,
            TenantId,
            CashAccountId,
            replacementAmount,
            replacementDirection,
            reasonCategory,
            reasonText,
            CashHandlerUserId,
            recordedByUserId,
            occurredAtUtc,
            PhysicalCashHandled).Post(postedAtUtc) with
        {
            OriginalMovementId = CashMovementId,
            CorrectionGroupId = correctionGroupId,
            CashTransferId = CashTransferId,
            TransferSide = TransferSide
        };

        return new CashMovementCorrection(original, reversal, replacement);
    }

    public decimal SignedAmount => Direction == CashMovementDirection.Inflow ? Amount.Amount : -Amount.Amount;

    public static CashMovementDirection Reverse(CashMovementDirection direction)
    {
        return direction switch
        {
            CashMovementDirection.Inflow => CashMovementDirection.Outflow,
            CashMovementDirection.Outflow => CashMovementDirection.Inflow,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported cash movement direction.")
        };
    }

    private static void ValidateCore(
        Guid cashMovementId,
        Guid cashAccountId,
        Money amount,
        string reasonCategory,
        UserId? cashHandlerUserId,
        bool physicalCashHandled)
    {
        if (cashMovementId == Guid.Empty)
        {
            throw new ArgumentException("Cash movement id is required.", nameof(cashMovementId));
        }

        if (cashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Cash account id is required.", nameof(cashAccountId));
        }

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount.Amount, "Cash movement amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(reasonCategory))
        {
            throw new ArgumentException("Cash movement reason/category is required.", nameof(reasonCategory));
        }

        if (physicalCashHandled && cashHandlerUserId is null)
        {
            throw new ArgumentException("Cash handler is required when physical cash is handled.", nameof(cashHandlerUserId));
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public sealed record CashMovementCorrection(
    CashMovement Original,
    CashMovement Reversal,
    CashMovement Replacement);
