namespace OpenCashFlow.Application.Payments.UpdatePayment;

public interface IUpdatePaymentOrchestrator
{
    Task<UpdatePaymentOrchestrationResult?> ExecuteAsync(UpdatePaymentCommand command, CancellationToken cancellationToken = default);
}
