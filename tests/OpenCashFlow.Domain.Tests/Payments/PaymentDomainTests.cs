using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Domain.Tests.Payments;

public sealed class PaymentDomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PaymentAmount_rejects_zero_or_negative(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaymentAmount.From(amount));
    }

    [Theory]
    [InlineData("Inflow", PaymentEntryType.InflowValue)]
    [InlineData("income", PaymentEntryType.InflowValue)]
    [InlineData("ENTRATA", PaymentEntryType.InflowValue)]
    [InlineData("Outflow", PaymentEntryType.OutflowValue)]
    [InlineData("expense", PaymentEntryType.OutflowValue)]
    [InlineData("USCITA", PaymentEntryType.OutflowValue)]
    public void PaymentEntryType_accepts_only_supported_values(string input, string expected)
    {
        var entryType = PaymentEntryType.From(input);

        Assert.Equal(expected, entryType.Value);
    }

    [Fact]
    public void PaymentEntryType_rejects_unknown_value()
    {
        Assert.Throws<ArgumentException>(() => PaymentEntryType.From("Transfer"));
    }

    [Fact]
    public void PaymentRules_returns_positive_delta_for_inflow()
    {
        var amount = PaymentAmount.From(100m);

        var delta = PaymentRules.GetCashDelta(amount, PaymentEntryType.Inflow);

        Assert.Equal(100m, delta);
    }

    [Fact]
    public void PaymentRules_returns_negative_delta_for_outflow()
    {
        var amount = PaymentAmount.From(100m);

        var delta = PaymentRules.GetCashDelta(amount, PaymentEntryType.Outflow);

        Assert.Equal(-100m, delta);
    }
}

