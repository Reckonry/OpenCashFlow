using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashAccount(
    Guid CashAccountId,
    TenantId TenantId,
    string Name,
    CashAccountType Type,
    string Currency,
    bool IsDefault,
    bool IsActive)
{
    public static CashAccount Create(
        Guid cashAccountId,
        TenantId tenantId,
        string name,
        CashAccountType type,
        string currency = "EUR",
        bool isDefault = false)
    {
        if (cashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Cash account id is required.", nameof(cashAccountId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Cash account name is required.", nameof(name));
        }

        return new CashAccount(
            cashAccountId,
            tenantId,
            name.Trim(),
            type,
            NormalizeCurrency(currency),
            isDefault,
            IsActive: true);
    }

    public CashAccount Deactivate() => this with { IsActive = false, IsDefault = false };

    private static string NormalizeCurrency(string currency)
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

        return normalizedCurrency;
    }
}
