using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Domain.Cash;

public sealed record CashSession(
    Guid CashSessionId,
    TenantId TenantId,
    Guid CashAccountId,
    DateOnly BusinessDate,
    Money OpeningExpectedBalance,
    Money? OpeningActualBalance,
    UserId OpenedByUserId,
    DateTimeOffset OpenedAtUtc,
    CashSessionStatus Status)
{
    public static CashSession Open(
        Guid cashSessionId,
        TenantId tenantId,
        Guid cashAccountId,
        DateOnly businessDate,
        Money openingExpectedBalance,
        Money? openingActualBalance,
        UserId openedByUserId,
        DateTimeOffset openedAtUtc)
    {
        if (cashSessionId == Guid.Empty)
        {
            throw new ArgumentException("Cash session id is required.", nameof(cashSessionId));
        }

        if (cashAccountId == Guid.Empty)
        {
            throw new ArgumentException("Cash account id is required.", nameof(cashAccountId));
        }

        return new CashSession(
            cashSessionId,
            tenantId,
            cashAccountId,
            businessDate,
            openingExpectedBalance,
            openingActualBalance,
            openedByUserId,
            openedAtUtc,
            CashSessionStatus.Open);
    }

    public Money CalculateExpectedBalance(IEnumerable<CashMovement> movements)
    {
        var balance = OpeningExpectedBalance;

        foreach (var movement in movements)
        {
            if (movement.TenantId != TenantId || movement.CashAccountId != CashAccountId)
            {
                throw new InvalidOperationException("Cash session can only calculate movements from the same tenant and cash account.");
            }

            if (movement.Status != CashMovementStatus.Posted)
            {
                continue;
            }

            balance = movement.Direction == CashMovementDirection.Inflow
                ? balance.Add(movement.Amount)
                : balance.Subtract(movement.Amount);
        }

        return balance;
    }

    public CashSession WithStatus(CashSessionStatus status) => this with { Status = status };
}
