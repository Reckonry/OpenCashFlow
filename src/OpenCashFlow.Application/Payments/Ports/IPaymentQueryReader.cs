using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IPaymentQueryReader
{
    Task<IReadOnlyList<PaymentListItem>> GetPaymentsAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default);

    Task<PaymentDetailResult?> GetPaymentDetailAsync(
        Guid paymentId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
