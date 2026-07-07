using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Application.Payments.GetPayments;

public sealed class GetPaymentsUseCase(IPaymentQueryReader paymentQueryReader) : IGetPaymentsUseCase
{
    public Task<IReadOnlyList<PaymentListItem>> ExecuteAsync(
        GetPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        TenantId.From(query.Query.TenantId);

        return paymentQueryReader.GetPaymentsAsync(query.Query.Normalize(), cancellationToken);
    }
}
