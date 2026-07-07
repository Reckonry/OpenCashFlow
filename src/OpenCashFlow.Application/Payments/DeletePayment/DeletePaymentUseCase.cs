using OpenCashFlow.Domain.Common;
using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Application.Payments.DeletePayment;

public sealed class DeletePaymentUseCase : IDeletePaymentUseCase
{
    public Task<DeletePaymentResult> ExecuteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(command);

        var paymentId = PaymentId.From(command.PaymentId);
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);

        return Task.FromResult(new DeletePaymentResult(
            paymentId.Value,
            tenantId.Value,
            userId.Value));
    }
}
