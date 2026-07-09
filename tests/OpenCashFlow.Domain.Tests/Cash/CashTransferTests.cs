using OpenCashFlow.Domain.Cash;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Tests.Cash;

public sealed class CashTransferTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly UserId User = UserId.New();
    private static readonly DateTimeOffset OccurredAt = new(2026, 7, 9, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset PostedAt = new(2026, 7, 9, 8, 1, 0, TimeSpan.Zero);

    [Fact]
    public void Create_rejects_same_source_and_destination_account()
    {
        var account = Account("Main cash");

        var exception = Assert.Throws<ArgumentException>(() =>
            CashTransfer.CreateDraft(
                Guid.NewGuid(),
                account,
                account,
                Money.Positive(100m),
                "employee-float",
                "Assign float",
                User,
                OccurredAt));

        Assert.Contains("cannot be the same", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_rejects_cross_tenant_transfer()
    {
        var source = Account("Main cash");
        var destination = Account("Other tenant", TenantId.New());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CashTransfer.CreateDraft(
                Guid.NewGuid(),
                source,
                destination,
                Money.Positive(100m),
                "employee-float",
                null,
                User,
                OccurredAt));

        Assert.Equal("Cash transfer cannot cross tenants.", exception.Message);
    }

    [Fact]
    public void Create_rejects_cross_currency_transfer()
    {
        var source = Account("Main cash", currency: "EUR");
        var destination = Account("USD box", currency: "USD");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            CashTransfer.CreateDraft(
                Guid.NewGuid(),
                source,
                destination,
                Money.Positive(100m, "EUR"),
                "employee-float",
                null,
                User,
                OccurredAt));

        Assert.Equal("Cash transfer cannot cross currencies in MVP.", exception.Message);
    }

    [Fact]
    public void Create_rejects_non_positive_amount()
    {
        var source = Account("Main cash");
        var destination = Account("Employee float", type: CashAccountType.EmployeeFloat);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CashTransfer.CreateDraft(
                Guid.NewGuid(),
                source,
                destination,
                Money.From(0m),
                "employee-float",
                null,
                User,
                OccurredAt));

        Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Posted_transfer_produces_two_linked_movement_facts()
    {
        var source = Account("Main cash");
        var destination = Account("Employee float", type: CashAccountType.EmployeeFloat);
        var transferId = Guid.NewGuid();

        var posting = CashTransfer.CreateDraft(
            transferId,
            source,
            destination,
            Money.Positive(300m),
            "employee-float",
            "Assign float",
            User,
            OccurredAt).Post(Guid.NewGuid(), Guid.NewGuid(), PostedAt);

        Assert.Equal(CashTransferStatus.Posted, posting.Transfer.Status);
        Assert.Equal(source.CashAccountId, posting.SourceMovement.CashAccountId);
        Assert.Equal(destination.CashAccountId, posting.DestinationMovement.CashAccountId);
        Assert.Equal(CashMovementDirection.Outflow, posting.SourceMovement.Direction);
        Assert.Equal(CashMovementDirection.Inflow, posting.DestinationMovement.Direction);
        Assert.Equal(transferId, posting.SourceMovement.CashTransferId);
        Assert.Equal(transferId, posting.DestinationMovement.CashTransferId);
        Assert.Equal(CashTransferSide.Source, posting.SourceMovement.TransferSide);
        Assert.Equal(CashTransferSide.Destination, posting.DestinationMovement.TransferSide);
    }

    [Fact]
    public void Posted_transfer_nets_to_zero_at_company_level()
    {
        var source = Account("Main cash");
        var destination = Account("Bank", type: CashAccountType.Bank);

        var posting = CashTransfer.CreateDraft(
            Guid.NewGuid(),
            source,
            destination,
            Money.Positive(1_000m),
            "bank-deposit",
            "Deposit cash to bank",
            User,
            OccurredAt).Post(Guid.NewGuid(), Guid.NewGuid(), PostedAt);

        var movementNet = posting.SourceMovement.SignedAmount + posting.DestinationMovement.SignedAmount;

        Assert.Equal(0m, movementNet);
        Assert.Equal(0m, posting.Transfer.CompanyLevelNetEffect);
    }

    [Fact]
    public void Reversal_reverses_both_transfer_sides()
    {
        var source = Account("Main cash");
        var destination = Account("Employee float", type: CashAccountType.EmployeeFloat);
        var transfer = CashTransfer.CreateDraft(
            Guid.NewGuid(),
            source,
            destination,
            Money.Positive(200m),
            "employee-float",
            "Assign float",
            User,
            OccurredAt).Post(Guid.NewGuid(), Guid.NewGuid(), PostedAt).Transfer;

        var reversal = transfer.CreatePostedReversal(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            User,
            "reversal",
            "Employee returned float",
            OccurredAt.AddHours(1),
            PostedAt.AddHours(1));

        Assert.Equal(CashTransferStatus.Reversed, reversal.Original.Status);
        Assert.Equal(transfer.CashTransferId, reversal.Reversal.OriginalTransferId);
        Assert.Equal(destination.CashAccountId, reversal.SourceMovement.CashAccountId);
        Assert.Equal(source.CashAccountId, reversal.DestinationMovement.CashAccountId);
        Assert.Equal(CashMovementDirection.Outflow, reversal.SourceMovement.Direction);
        Assert.Equal(CashMovementDirection.Inflow, reversal.DestinationMovement.Direction);
        Assert.Equal(0m, reversal.SourceMovement.SignedAmount + reversal.DestinationMovement.SignedAmount);
    }

    [Fact]
    public void Correction_preserves_original_reversal_and_replacement_transfers()
    {
        var source = Account("Main cash");
        var destination = Account("Employee float", type: CashAccountType.EmployeeFloat);
        var transfer = CashTransfer.CreateDraft(
            Guid.NewGuid(),
            source,
            destination,
            Money.Positive(200m),
            "employee-float",
            "Assign float",
            User,
            OccurredAt).Post(Guid.NewGuid(), Guid.NewGuid(), PostedAt).Transfer;

        var correction = transfer.CreateCorrection(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            source,
            destination,
            Money.Positive(250m),
            "correction",
            "Correct float amount",
            User,
            OccurredAt.AddMinutes(10),
            PostedAt.AddMinutes(10));

        Assert.Equal(CashTransferStatus.Corrected, correction.Original.Status);
        Assert.Equal(transfer.CashTransferId, correction.Reversal.OriginalTransferId);
        Assert.Equal(transfer.CashTransferId, correction.Replacement.OriginalTransferId);
        Assert.Equal(correction.Original.CorrectionGroupId, correction.Reversal.CorrectionGroupId);
        Assert.Equal(correction.Original.CorrectionGroupId, correction.Replacement.CorrectionGroupId);
        Assert.Equal(250m, correction.Replacement.Amount.Amount);
        Assert.Equal(0m, correction.ReplacementSourceMovement.SignedAmount + correction.ReplacementDestinationMovement.SignedAmount);
    }

    private static CashAccount Account(
        string name,
        TenantId? tenant = null,
        string currency = "EUR",
        CashAccountType type = CashAccountType.PhysicalCash)
    {
        return CashAccount.Create(Guid.NewGuid(), tenant ?? Tenant, name, type, currency);
    }
}
