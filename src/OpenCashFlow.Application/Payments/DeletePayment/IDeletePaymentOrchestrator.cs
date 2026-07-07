namespace OpenCashFlow.Application.Payments.DeletePayment;

public interface IDeletePaymentOrchestrator
{
    Task<DeletePaymentOrchestrationResult> ExecuteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default);
}
