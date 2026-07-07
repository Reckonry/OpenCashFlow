using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Payments.Reports;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        public async Task<double> GetDailyPaymentsAsync(Guid TenantID, DateTime date, CancellationToken cancellationToken)
        {
            return await _getPaymentReportsUseCase.GetDailyPaymentAsync(
                new GetDailyPaymentQuery(TenantID, date),
                cancellationToken);
        }

        public async Task<double> GetTotalPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await _getPaymentReportsUseCase.GetTotalInPeriodAsync(
                new GetPaymentPeriodReportQuery(TenantID, startDate, endDate),
                cancellationToken);
        }

        public async Task<IEnumerable<Payment_DailyPayments>> GetDailyPaymentsInPeriodAsync(Guid TenantID, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var payments = await _getPaymentReportsUseCase.GetDailyPaymentsInPeriodAsync(
                new GetPaymentPeriodReportQuery(TenantID, startDate, endDate),
                cancellationToken);

            return payments.Select(ToDailyPaymentModel).ToList();
        }
        public async Task<double> GetMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            return await _getPaymentReportsUseCase.GetMonthlyPaymentsAsync(
                new GetPaymentMonthReportQuery(TenantID, year, month),
                cancellationToken);
        }
        public async Task<IEnumerable<Payment_DailyPayments>> GetAllMonthlyPaymentsAsync(Guid TenantID, int year, int month, CancellationToken cancellationToken)
        {
            var payments = await _getPaymentReportsUseCase.GetAllMonthlyPaymentsAsync(
                new GetPaymentMonthReportQuery(TenantID, year, month),
                cancellationToken);

            return payments.Select(ToDailyPaymentModel).ToList();
        }
        public async Task<double> GetYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            return await _getPaymentReportsUseCase.GetYearlyPaymentsAsync(
                new GetPaymentYearReportQuery(TenantID, year),
                cancellationToken);
        }
        public async Task<IEnumerable<Payment_DailyPayments>> GetAllYearlyPaymentsAsync(Guid TenantID, int year, CancellationToken cancellationToken)
        {
            var payments = await _getPaymentReportsUseCase.GetAllYearlyPaymentsAsync(
                new GetPaymentYearReportQuery(TenantID, year),
                cancellationToken);

            return payments.Select(ToDailyPaymentModel).ToList();
        }

    }
}
