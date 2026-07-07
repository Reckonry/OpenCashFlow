namespace OpenCashFlow.Application.Payments.CreatePayment;

public interface ICreatePaymentOrchestrator
{
    Task<CreatePaymentOrchestrationResult?> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default);
}
