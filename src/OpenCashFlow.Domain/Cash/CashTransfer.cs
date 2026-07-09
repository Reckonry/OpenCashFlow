using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashTransfer(
    Guid CashTransferId,
    TenantId TenantId,
    Guid SourceCashAccountId,
    Guid DestinationCashAccountId,
    Money Amount,
    string ReasonCategory,
    string? ReasonText,
    UserId RecordedByUserId,
    UserId? ApprovedByUserId,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset? PostedAtUtc,
    CashTransferStatus Status,
    Guid? OriginalTransferId,
    Guid? CorrectionGroupId)
{
    public static CashTransfer CreateDraft(
        Guid cashTransferId,
        CashAccount sourceAccount,
        CashAccount destinationAccount,
        Money amount,
        string reasonCategory,
        string? reasonText,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc)
    {
        ValidateAccounts(sourceAccount, destinationAccount);
        ValidateCore(cashTransferId, amount, reasonCategory);

        return new CashTransfer(
            cashTransferId,
            sourceAccount.TenantId,
            sourceAccount.CashAccountId,
            destinationAccount.CashAccountId,
            amount,
            reasonCategory.Trim(),
            NormalizeOptional(reasonText),
            recordedByUserId,
            ApprovedByUserId: null,
            occurredAtUtc,
            PostedAtUtc: null,
            CashTransferStatus.Draft,
            OriginalTransferId: null,
            CorrectionGroupId: null);
    }

    public CashTransferPosting Post(
        Guid sourceMovementId,
        Guid destinationMovementId,
        DateTimeOffset postedAtUtc,
        UserId? approvedByUserId = null)
    {
        if (Status != CashTransferStatus.Draft)
        {
            throw new InvalidOperationException("Only draft cash transfers can be posted.");
        }

        var postedTransfer = this with
        {
            Status = CashTransferStatus.Posted,
            PostedAtUtc = postedAtUtc,
            ApprovedByUserId = approvedByUserId
        };

        var sourceMovement = CashMovement.CreatePostedTransferSide(
            sourceMovementId,
            TenantId,
            SourceCashAccountId,
            Amount,
            CashMovementDirection.Outflow,
            CashTransferId,
            CashTransferSide.Source,
            ReasonCategory,
            ReasonText,
            RecordedByUserId,
            approvedByUserId,
            OccurredAtUtc,
            postedAtUtc);

        var destinationMovement = CashMovement.CreatePostedTransferSide(
            destinationMovementId,
            TenantId,
            DestinationCashAccountId,
            Amount,
            CashMovementDirection.Inflow,
            CashTransferId,
            CashTransferSide.Destination,
            ReasonCategory,
            ReasonText,
            RecordedByUserId,
            approvedByUserId,
            OccurredAtUtc,
            postedAtUtc);

        return new CashTransferPosting(postedTransfer, sourceMovement, destinationMovement);
    }

    public CashTransferReversal CreatePostedReversal(
        Guid reversalTransferId,
        Guid sourceReversalMovementId,
        Guid destinationReversalMovementId,
        UserId recordedByUserId,
        string reasonCategory,
        string reasonText,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc)
    {
        if (Status != CashTransferStatus.Posted)
        {
            throw new InvalidOperationException("Only posted cash transfers can be reversed.");
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            throw new ArgumentException("Transfer reversal reason is required.", nameof(reasonText));
        }

        var correctionGroupId = CorrectionGroupId ?? Guid.NewGuid();
        var original = this with
        {
            Status = CashTransferStatus.Reversed,
            CorrectionGroupId = correctionGroupId
        };

        var reversal = CreateRawPostedReversal(
            reversalTransferId,
            sourceAccountId: DestinationCashAccountId,
            destinationAccountId: SourceCashAccountId,
            amount: Amount,
            tenantId: TenantId,
            reasonCategory,
            reasonText,
            recordedByUserId,
            occurredAtUtc,
            postedAtUtc,
            originalTransferId: CashTransferId,
            correctionGroupId);

        var sourceMovement = CashMovement.CreatePostedTransferSide(
            sourceReversalMovementId,
            TenantId,
            reversal.SourceCashAccountId,
            Amount,
            CashMovementDirection.Outflow,
            reversal.CashTransferId,
            CashTransferSide.Source,
            reasonCategory,
            reasonText,
            recordedByUserId,
            approvedByUserId: null,
            occurredAtUtc,
            postedAtUtc);

        var destinationMovement = CashMovement.CreatePostedTransferSide(
            destinationReversalMovementId,
            TenantId,
            reversal.DestinationCashAccountId,
            Amount,
            CashMovementDirection.Inflow,
            reversal.CashTransferId,
            CashTransferSide.Destination,
            reasonCategory,
            reasonText,
            recordedByUserId,
            approvedByUserId: null,
            occurredAtUtc,
            postedAtUtc);

        return new CashTransferReversal(original, reversal, sourceMovement, destinationMovement);
    }

    public CashTransferCorrection CreateCorrection(
        Guid reversalTransferId,
        Guid reversalSourceMovementId,
        Guid reversalDestinationMovementId,
        Guid replacementTransferId,
        Guid replacementSourceMovementId,
        Guid replacementDestinationMovementId,
        CashAccount replacementSourceAccount,
        CashAccount replacementDestinationAccount,
        Money replacementAmount,
        string reasonCategory,
        string reasonText,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc)
    {
        if (Status != CashTransferStatus.Posted)
        {
            throw new InvalidOperationException("Only posted cash transfers can be corrected.");
        }

        var correctionGroupId = Guid.NewGuid();
        var reversal = CreatePostedReversal(
            reversalTransferId,
            reversalSourceMovementId,
            reversalDestinationMovementId,
            recordedByUserId,
            reasonCategory,
            reasonText,
            occurredAtUtc,
            postedAtUtc);

        var original = reversal.Original with
        {
            Status = CashTransferStatus.Corrected,
            CorrectionGroupId = correctionGroupId
        };

        var correctedReversal = reversal.Reversal with { CorrectionGroupId = correctionGroupId };
        var replacementPosting = CreateDraft(
            replacementTransferId,
            replacementSourceAccount,
            replacementDestinationAccount,
            replacementAmount,
            reasonCategory,
            reasonText,
            recordedByUserId,
            occurredAtUtc).Post(
                replacementSourceMovementId,
                replacementDestinationMovementId,
                postedAtUtc) with
            {
                Transfer = CreateRawPostedReplacement(
                    replacementTransferId,
                    replacementSourceAccount,
                    replacementDestinationAccount,
                    replacementAmount,
                    reasonCategory,
                    reasonText,
                    recordedByUserId,
                    occurredAtUtc,
                    postedAtUtc,
                    originalTransferId: CashTransferId,
                    correctionGroupId)
            };

        return new CashTransferCorrection(
            original,
            correctedReversal,
            replacementPosting.Transfer,
            reversal.SourceMovement,
            reversal.DestinationMovement,
            replacementPosting.SourceMovement,
            replacementPosting.DestinationMovement);
    }

    public decimal CompanyLevelNetEffect => 0m;

    private static void ValidateAccounts(CashAccount sourceAccount, CashAccount destinationAccount)
    {
        if (sourceAccount.CashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Source cash account id is required.", nameof(sourceAccount));
        }

        if (destinationAccount.CashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Destination cash account id is required.", nameof(destinationAccount));
        }

        if (sourceAccount.CashAccountId == destinationAccount.CashAccountId)
        {
            throw new ArgumentException("Source and destination cash accounts cannot be the same.", nameof(destinationAccount));
        }

        if (sourceAccount.TenantId != destinationAccount.TenantId)
        {
            throw new InvalidOperationException("Cash transfer cannot cross tenants.");
        }

        if (!string.Equals(sourceAccount.Currency, destinationAccount.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cash transfer cannot cross currencies in MVP.");
        }
    }

    private static void ValidateCore(Guid cashTransferId, Money amount, string reasonCategory)
    {
        if (cashTransferId == Guid.Empty)
        {
            throw new ArgumentException("Cash transfer id is required.", nameof(cashTransferId));
        }

        if (amount.Amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount.Amount, "Cash transfer amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(reasonCategory))
        {
            throw new ArgumentException("Cash transfer reason/category is required.", nameof(reasonCategory));
        }
    }

    private static CashTransfer CreateRawPostedReversal(
        Guid cashTransferId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        Money amount,
        TenantId tenantId,
        string reasonCategory,
        string reasonText,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc,
        Guid originalTransferId,
        Guid correctionGroupId)
    {
        ValidateCore(cashTransferId, amount, reasonCategory);

        return new CashTransfer(
            cashTransferId,
            tenantId,
            sourceAccountId,
            destinationAccountId,
            amount,
            reasonCategory.Trim(),
            reasonText.Trim(),
            recordedByUserId,
            ApprovedByUserId: null,
            occurredAtUtc,
            postedAtUtc,
            CashTransferStatus.Posted,
            originalTransferId,
            correctionGroupId);
    }

    private static CashTransfer CreateRawPostedReplacement(
        Guid cashTransferId,
        CashAccount sourceAccount,
        CashAccount destinationAccount,
        Money amount,
        string reasonCategory,
        string reasonText,
        UserId recordedByUserId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset postedAtUtc,
        Guid originalTransferId,
        Guid correctionGroupId)
    {
        ValidateAccounts(sourceAccount, destinationAccount);
        ValidateCore(cashTransferId, amount, reasonCategory);

        return new CashTransfer(
            cashTransferId,
            sourceAccount.TenantId,
            sourceAccount.CashAccountId,
            destinationAccount.CashAccountId,
            amount,
            reasonCategory.Trim(),
            reasonText.Trim(),
            recordedByUserId,
            ApprovedByUserId: null,
            occurredAtUtc,
            postedAtUtc,
            CashTransferStatus.Posted,
            originalTransferId,
            correctionGroupId);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public sealed record CashTransferPosting(
    CashTransfer Transfer,
    CashMovement SourceMovement,
    CashMovement DestinationMovement);

public sealed record CashTransferReversal(
    CashTransfer Original,
    CashTransfer Reversal,
    CashMovement SourceMovement,
    CashMovement DestinationMovement);

public sealed record CashTransferCorrection(
    CashTransfer Original,
    CashTransfer Reversal,
    CashTransfer Replacement,
    CashMovement ReversalSourceMovement,
    CashMovement ReversalDestinationMovement,
    CashMovement ReplacementSourceMovement,
    CashMovement ReplacementDestinationMovement);
