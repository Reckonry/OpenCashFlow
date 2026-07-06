using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Tests.Common;

public sealed class MoneyTests
{
    [Fact]
    public void NonNegative_rejects_negative_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.NonNegative(-0.01m));
    }

    [Fact]
    public void NonNegative_accepts_zero()
    {
        var money = Money.NonNegative(0m, "eur");

        Assert.Equal(0m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Positive_rejects_zero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.Positive(0m));
    }

    [Fact]
    public void Add_rejects_different_currencies()
    {
        var eur = Money.NonNegative(10m, "EUR");
        var usd = Money.NonNegative(10m, "USD");

        Assert.Throws<InvalidOperationException>(() => eur.Add(usd));
    }
}

