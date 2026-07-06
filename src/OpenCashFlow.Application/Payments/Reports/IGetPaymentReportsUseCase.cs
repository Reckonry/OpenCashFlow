using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.Reports;

public interface IGetPaymentReportsUseCase
{
    Task<double> GetDailyPaymentAsync(GetDailyPaymentQuery query, CancellationToken cancellationToken = default);

    Task<double> GetTotalInPeriodAsync(GetPaymentPeriodReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetDailyPaymentsInPeriodAsync(GetPaymentPeriodReportQuery query, CancellationToken cancellationToken = default);

    Task<double> GetMonthlyPaymentsAsync(GetPaymentMonthReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetAllMonthlyPaymentsAsync(GetPaymentMonthReportQuery query, CancellationToken cancellationToken = default);

    Task<double> GetYearlyPaymentsAsync(GetPaymentYearReportQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetAllYearlyPaymentsAsync(GetPaymentYearReportQuery query, CancellationToken cancellationToken = default);
}
