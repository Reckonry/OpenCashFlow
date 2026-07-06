namespace OpenCashFlow.Domain.Common;

public readonly record struct Money
{
    private Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money From(decimal amount, string currency = "EUR") => new(amount, currency);

    public static Money NonNegative(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Money amount cannot be negative.");
        }

        return new Money(amount, currency);
    }

    public static Money Positive(decimal amount, string currency = "EUR")
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Money amount must be greater than zero.");
        }

        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return From(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return From(Amount - other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cannot operate on money values with different currencies.");
        }
    }
}

