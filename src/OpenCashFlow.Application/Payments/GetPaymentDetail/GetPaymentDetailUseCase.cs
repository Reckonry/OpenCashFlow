using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Domain.Common;
using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Application.Payments.GetPaymentDetail;

public sealed class GetPaymentDetailUseCase(IPaymentQueryReader paymentQueryReader) : IGetPaymentDetailUseCase
{
    public Task<PaymentDetailResult?> ExecuteAsync(
        GetPaymentDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var paymentId = PaymentId.From(query.PaymentId);
        var tenantId = TenantId.From(query.TenantId);

        return paymentQueryReader.GetPaymentDetailAsync(paymentId.Value, tenantId.Value, cancellationToken);
    }
}
