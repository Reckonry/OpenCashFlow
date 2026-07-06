using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IPaymentCalendarReader
{
    Task<IReadOnlyList<PaymentCalendarEventResult>> GetPaymentCalendarEventsAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default);
}
