using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Persistence;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Payments.Persistence;

public sealed class DailyPaymentPersistence(ApplicationDbContext context) : IDailyPaymentPersistence, IDailyPaymentWriter
{
    public async Task UpdateDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
    {
        var cash = await context.DailyCash_DS
            .FirstOrDefaultAsync(c => c.TenantID == tenantId && c.CashDate == date.Date, cancellationToken);

        var delta = entryType == nameof(EntryTypeEnum.Income)
            ? Convert.ToDouble(amount)
            : -Convert.ToDouble(amount);

        if (cash == null)
        {
            cash = new Payment_DailyPayments
            {
                TenantID = tenantId,
                CashDate = date.Date,
                Total = delta
            };
            context.DailyCash_DS.Add(cash);
        }
        else
        {
            cash.Total += delta;
            context.DailyCash_DS.Update(cash);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
    {
        var cash = await context.DailyCash_DS
            .FirstOrDefaultAsync(c => c.TenantID == tenantId && c.CashDate == date.Date, cancellationToken);

        var delta = entryType == nameof(EntryTypeEnum.Income)
            ? -Convert.ToDouble(amount)
            : Convert.ToDouble(amount);

        if (cash != null)
        {
            cash.Total += delta;
            context.DailyCash_DS.Update(cash);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
