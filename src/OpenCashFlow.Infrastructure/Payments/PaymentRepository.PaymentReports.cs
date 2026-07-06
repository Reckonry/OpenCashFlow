using OpenCashFlow.Application.Payments.Repositories;
using Microsoft.EntityFrameworkCore;
using global::Shared.Models;

namespace OpenCashFlow.Infrastructure.Payments
{
    public partial class PaymentRepository : IPaymentRepository
    {
        public async Task<double> GetDailyPaymentAsync(Guid TenantID, DateTime date, CancellationToken cancellationToken)
        {
            return (await _context.DailyCash_DS
                .FirstOrDefaultAsync(c => c.TenantID == TenantID && c.CashDate == date.Date, cancellationToken) ?? new()).Total;
        }
        public async Task<IEnumerable<Payment_DailyPayments>> GetAllDailyPaymentsAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID)
                .ToListAsync(cancellationToken);
        }

        public async Task<Payment_DailyPayments> UpdateDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken)
        {
            var cash = await _context.DailyCash_DS
                .FirstOrDefaultAsync(c => c.TenantID == TenantID && c.CashDate == date.Date, cancellationToken);

            var delta = entryType == nameof(EntryTypeEnum.Income) ? amount : -amount;

            if (cash == null)
            {
                cash = new Payment_DailyPayments
                {
                    TenantID = TenantID,
                    CashDate = date.Date,
                    Total = delta
                };
                _context.DailyCash_DS.Add(cash);
            }
            else
            {
                cash.Total += delta;
                _context.DailyCash_DS.Update(cash);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return cash;
        }

        public async Task<bool> DeleteDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken)
        {
            var cash = await _context.DailyCash_DS
                .FirstOrDefaultAsync(c => c.TenantID == TenantID && c.CashDate == date.Date, cancellationToken);

            var delta = entryType == nameof(EntryTypeEnum.Income) ? -amount : amount;

            if (cash != null)          
            {
                cash.Total += delta;
                _context.DailyCash_DS.Update(cash);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }


        public async Task<double> GetTotalPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate >= startDate.Date && c.CashDate <= endDate.Date)
                .SumAsync(c => c.Total, cancellationToken);
        }

        public async Task<IEnumerable<Payment_DailyPayments>> GetDailyPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate >= startDate.Date && c.CashDate <= endDate.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<double> GetMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate.Year == year && c.CashDate.Month == month)
                .SumAsync(c => c.Total, cancellationToken);
        }

        public async Task<IEnumerable<Payment_DailyPayments>> GetAllMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate.Year == year && c.CashDate.Month == month)
                .ToListAsync(cancellationToken);
        }

        public async Task<double> GetYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate.Year == year)
                .SumAsync(c => c.Total, cancellationToken);
        }

        public async Task<IEnumerable<Payment_DailyPayments>> GetAllYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            return await _context.DailyCash_DS
                .Where(c => c.TenantID == TenantID && c.CashDate.Year == year)
                .ToListAsync(cancellationToken);
        }

    }
}
