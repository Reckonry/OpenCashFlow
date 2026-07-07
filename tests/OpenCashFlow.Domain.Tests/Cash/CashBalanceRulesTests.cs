using OpenCashFlow.Domain.Cash;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Tests.Cash;

public sealed class CashBalanceRulesTests
{
    [Fact]
    public void Inflow_increases_balance()
    {
        var balance = Money.NonNegative(50m);
        var amount = Money.NonNegative(25m);

        var updated = CashBalanceRules.Apply(balance, amount, CashLedgerEntryType.Inflow);

        Assert.Equal(75m, updated.Amount);
    }

    [Fact]
    public void Outflow_decreases_balance()
    {
        var balance = Money.NonNegative(50m);
        var amount = Money.NonNegative(25m);

        var updated = CashBalanceRules.Apply(balance, amount, CashLedgerEntryType.Outflow);

        Assert.Equal(25m, updated.Amount);
    }

    [Fact]
    public void Outflow_can_produce_negative_balance()
    {
        var balance = Money.NonNegative(10m);
        var amount = Money.NonNegative(25m);

        var updated = CashBalanceRules.Apply(balance, amount, CashLedgerEntryType.Outflow);

        Assert.Equal(-15m, updated.Amount);
    }
}

