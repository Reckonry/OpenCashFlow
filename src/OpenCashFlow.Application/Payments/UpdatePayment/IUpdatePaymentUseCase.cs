namespace OpenCashFlow.Application.Payments.UpdatePayment;

public interface IUpdatePaymentUseCase
{
    Task<UpdatePaymentResult> ExecuteAsync(UpdatePaymentCommand command, CancellationToken cancellationToken = default);
}
