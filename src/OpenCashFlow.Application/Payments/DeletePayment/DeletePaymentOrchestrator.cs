using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.DeletePayment;

public sealed class DeletePaymentOrchestrator(
    IDeletePaymentUseCase deletePaymentUseCase,
    IPaymentReader paymentReader,
    IPaymentWriter paymentWriter,
    IDailyPaymentWriter dailyPaymentWriter,
    ICashLedgerWriter cashLedgerWriter,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IDeletePaymentOrchestrator
{
    private static readonly Guid SystemCashPaymentMethodId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly string[] CashMethodAliases = ["Cash", "Contanti"];

    public async Task<DeletePaymentOrchestrationResult> ExecuteAsync(DeletePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var validatedPayment = await deletePaymentUseCase.ExecuteAsync(command, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var payment = await paymentReader.GetByIdAsync(
                validatedPayment.PaymentId,
                validatedPayment.TenantId,
                ct);

            if (payment == null)
            {
                return new DeletePaymentOrchestrationResult(
                    Deleted: false,
                    Payment: null,
                    DailyPaymentApplied: false,
                    CashLedgerApplied: false);
            }

            var deleted = await paymentWriter.DeleteAsync(
                validatedPayment.PaymentId,
                validatedPayment.TenantId,
                ct);

            if (!deleted)
            {
                return new DeletePaymentOrchestrationResult(
                    Deleted: false,
                    Payment: payment,
                    DailyPaymentApplied: false,
                    CashLedgerApplied: false);
            }

            await dailyPaymentWriter.DeleteDailyPaymentAsync(
                payment.TenantId,
                payment.DateIns,
                payment.Amount,
                payment.EntryType,
                ct);

            var paymentMethod = await paymentReader.GetPaymentMethodByIdAsync(
                payment.PaymentMethodId,
                payment.TenantId,
                ct);

            var cashLedgerApplied = false;
            if (IsCashMethod(payment.PaymentMethodId, paymentMethod))
            {
                await cashLedgerWriter.VoidPaymentAsync(
                    payment.TenantId,
                    payment.PaymentId,
                    payment.Amount,
                    validatedPayment.UserId,
                    ct);

                cashLedgerApplied = true;
            }

            await auditWriter.WritePaymentDeletedAsync(
                new PaymentDeletedAudit(payment, validatedPayment.UserId),
                ct);

            return new DeletePaymentOrchestrationResult(
                Deleted: true,
                Payment: payment,
                DailyPaymentApplied: true,
                CashLedgerApplied: cashLedgerApplied);
        }, cancellationToken);
    }

    private static bool IsCashMethod(Guid paymentMethodId, PaymentMethodSnapshot? method)
    {
        if (paymentMethodId == SystemCashPaymentMethodId || method?.PaymentMethodId == SystemCashPaymentMethodId)
        {
            return true;
        }

        var name = method?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var alias in CashMethodAliases)
        {
            if (string.Equals(name, alias, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
