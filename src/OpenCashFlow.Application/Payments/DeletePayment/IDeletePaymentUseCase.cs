namespace OpenCashFlow.Application.Payments.DeletePayment;

public interface IDeletePaymentUseCase
{
    Task<DeletePaymentResult> ExecuteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default);
}
