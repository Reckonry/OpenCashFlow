using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IPaymentReportReader
{
    Task<double> GetDailyPaymentAsync(Guid tenantId, DateTime date, CancellationToken cancellationToken = default);

    Task<double> GetTotalPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetDailyPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<double> GetMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetAllMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default);

    Task<double> GetYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyPaymentResult>> GetAllYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default);
}
