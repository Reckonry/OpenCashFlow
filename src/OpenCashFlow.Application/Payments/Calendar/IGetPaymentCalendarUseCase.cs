using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.Calendar;

public interface IGetPaymentCalendarUseCase
{
    Task<IReadOnlyList<PaymentCalendarEventResult>> ExecuteAsync(
        GetPaymentCalendarQuery query,
        CancellationToken cancellationToken = default);
}
