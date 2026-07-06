using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Payments;

public readonly record struct PaymentAmount
{
    public PaymentAmount(Money value)
    {
        if (value.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value.Amount, "Payment amount must be greater than zero.");
        }

        Value = value;
    }

    public Money Value { get; }

    public decimal Amount => Value.Amount;

    public string Currency => Value.Currency;

    public static PaymentAmount From(decimal amount, string currency = "EUR") => new(Money.Positive(amount, currency));

    public override string ToString() => Value.ToString();
}

