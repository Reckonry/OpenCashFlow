namespace OpenCashFlow.Application.Payments.CreatePayment;

public interface ICreatePaymentUseCase
{
    Task<CreatePaymentResult> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default);
}
