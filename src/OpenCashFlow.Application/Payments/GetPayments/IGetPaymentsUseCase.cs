using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Payments.GetPayments;

public interface IGetPaymentsUseCase
{
    Task<IReadOnlyList<PaymentListItem>> ExecuteAsync(
        GetPaymentsQuery query,
        CancellationToken cancellationToken = default);
}
