using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Application.Payments.Reports;

namespace OpenCashFlow.Application.Tests.Payments.Reports;

public sealed class GetPaymentReportsUseCaseTests
{
    [Fact]
    public async Task GetDailyPaymentAsync_PassesTenantAndDateToReader()
    {
        var reader = new FakePaymentReportReader();
        var useCase = new GetPaymentReportsUseCase(reader);
        var tenantId = Guid.NewGuid();
        var date = new DateTime(2026, 7, 6, 10, 30, 0, DateTimeKind.Utc);

        await useCase.GetDailyPaymentAsync(new GetDailyPaymentQuery(tenantId, date));

        Assert.Equal(tenantId, reader.LastTenantId);
        Assert.Equal(date.Date, reader.LastDate);
    }

    [Fact]
    public async Task GetTotalInPeriodAsync_WithInvalidPeriod_Fails()
    {
        var useCase = new GetPaymentReportsUseCase(new FakePaymentReportReader());

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.GetTotalInPeriodAsync(
            new GetPaymentPeriodReportQuery(Guid.NewGuid(), new DateTime(2026, 7, 7), new DateTime(2026, 7, 6))));
    }

    [Fact]
    public async Task GetMonthlyPaymentsAsync_WithInvalidMonth_Fails()
    {
        var useCase = new GetPaymentReportsUseCase(new FakePaymentReportReader());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.GetMonthlyPaymentsAsync(
            new GetPaymentMonthReportQuery(Guid.NewGuid(), 2026, 13)));
    }

    private sealed class FakePaymentReportReader : IPaymentReportReader
    {
        public Guid LastTenantId { get; private set; }
        public DateTime LastDate { get; private set; }

        public Task<double> GetDailyPaymentAsync(Guid tenantId, DateTime date, CancellationToken cancellationToken = default)
        {
            LastTenantId = tenantId;
            LastDate = date;
            return Task.FromResult(0d);
        }

        public Task<double> GetTotalPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
            => Task.FromResult(0d);

        public Task<IReadOnlyList<DailyPaymentResult>> GetDailyPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DailyPaymentResult>>([]);

        public Task<double> GetMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default)
            => Task.FromResult(0d);

        public Task<IReadOnlyList<DailyPaymentResult>> GetAllMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DailyPaymentResult>>([]);

        public Task<double> GetYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default)
            => Task.FromResult(0d);

        public Task<IReadOnlyList<DailyPaymentResult>> GetAllYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DailyPaymentResult>>([]);
    }
}
