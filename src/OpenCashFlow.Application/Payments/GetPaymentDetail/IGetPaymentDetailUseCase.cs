using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.GetPaymentDetail;

public interface IGetPaymentDetailUseCase
{
    Task<PaymentDetailResult?> ExecuteAsync(
        GetPaymentDetailQuery query,
        CancellationToken cancellationToken = default);
}
