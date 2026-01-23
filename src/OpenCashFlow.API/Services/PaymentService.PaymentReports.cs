using OpenCashFlow.API.Services.Interfaces;
using global::Shared.Models;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<double> GetDailyPaymentsAsync(Guid TenantID, DateTime date, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetDailyPaymentAsync(TenantID, date.Date, cancellationToken);
        }

        public async Task UpdateDailyPaymentAsync(Guid TenantID, DateTime date, double amount, string entryType, CancellationToken cancellationToken)
        {
            await _paymentRepository.UpdateDailyPaymentAsync(TenantID, date.Date, amount, entryType, cancellationToken);
        }

        public async Task DeleteDailyPaymentAsync(Guid TenantID, double amount, DateTime date, string entryType, CancellationToken cancellationToken)
        {
            await _paymentRepository.DeleteDailyPaymentAsync(TenantID, date.Date, amount, entryType, cancellationToken);
        }

        public async Task<double> GetTotalPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetTotalPaymentsInPeriodAsync(TenantID, startDate.Date, endDate.Date, cancellationToken);
        }

        public async Task<IEnumerable<Payment_DailyPayments>> GetDailyPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetDailyPaymentsInPeriodAsync(TenantID, startDate.Date, endDate.Date, cancellationToken);
        }
        public async Task<double> GetMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetMonthlyPaymentsAsync(TenantID, year, month, cancellationToken);
        }
        public async Task<IEnumerable<Payment_DailyPayments>> GetAllMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetAllMonthlyPaymentsAsync(TenantID, year, month, cancellationToken);
        }
        public async Task<double> GetYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetYearlyPaymentsAsync(TenantID, year, cancellationToken);
        }
        public async Task<IEnumerable<Payment_DailyPayments>> GetAllYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            return await _paymentRepository.GetAllYearlyPaymentsAsync(TenantID, year, cancellationToken);
        }

    }
}
