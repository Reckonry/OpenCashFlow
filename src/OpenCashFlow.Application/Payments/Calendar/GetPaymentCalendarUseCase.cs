using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Application.Payments.Calendar;

public sealed class GetPaymentCalendarUseCase(IPaymentCalendarReader paymentCalendarReader) : IGetPaymentCalendarUseCase
{
    public Task<IReadOnlyList<PaymentCalendarEventResult>> ExecuteAsync(
        GetPaymentCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        TenantId.From(query.Query.TenantId);

        return paymentCalendarReader.GetPaymentCalendarEventsAsync(query.Query.Normalize(), cancellationToken);
    }
}
