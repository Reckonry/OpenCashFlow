using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.CreatePayment;

public sealed class CreatePaymentOrchestrator(
    ICreatePaymentUseCase createPaymentUseCase,
    IPaymentReader paymentReader,
    IPaymentWriter paymentWriter,
    IDailyPaymentWriter dailyPaymentWriter,
    ICashLedgerWriter cashLedgerWriter,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : ICreatePaymentOrchestrator
{
    private static readonly Guid SystemCashPaymentMethodId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly string[] CashMethodAliases = ["Cash", "Contanti"];

    public async Task<CreatePaymentOrchestrationResult?> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var validatedPayment = await createPaymentUseCase.ExecuteAsync(command, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var existingPayment = await paymentReader.GetByRequestIdAsync(
                validatedPayment.RequestId,
                validatedPayment.TenantId,
                ct);

            if (existingPayment != null)
            {
                return new CreatePaymentOrchestrationResult(
                    existingPayment,
                    validatedPayment,
                    WasExisting: true,
                    DailyPaymentApplied: false,
                    CashLedgerApplied: false);
            }

            var paymentDraft = new PaymentDraft(
                validatedPayment.PaymentId,
                validatedPayment.TenantId,
                validatedPayment.RequestId,
                validatedPayment.Amount,
                validatedPayment.NormalizedEntryType,
                validatedPayment.PaymentMethodId,
                validatedPayment.DocumentTypeId,
                validatedPayment.UserId,
                validatedPayment.DateIns,
                command.Description);

            var createdPayment = await paymentWriter.CreateAsync(paymentDraft, ct);
            if (createdPayment == null)
            {
                return null;
            }

            await dailyPaymentWriter.UpdateDailyPaymentAsync(
                createdPayment.TenantId,
                createdPayment.DateIns,
                createdPayment.Amount,
                createdPayment.EntryType,
                ct);

            var paymentMethod = await paymentReader.GetPaymentMethodByIdAsync(
                validatedPayment.PaymentMethodId,
                validatedPayment.TenantId,
                ct);

            var cashLedgerApplied = false;
            if (IsCashMethod(validatedPayment.PaymentMethodId, paymentMethod))
            {
                await cashLedgerWriter.ApplyPaymentAsync(
                    validatedPayment.TenantId,
                    createdPayment.PaymentId,
                    validatedPayment.CashDelta,
                    validatedPayment.UserId,
                    ct);

                cashLedgerApplied = true;
            }

            await auditWriter.WritePaymentCreatedAsync(createdPayment, ct);

            return new CreatePaymentOrchestrationResult(
                createdPayment,
                validatedPayment,
                WasExisting: false,
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
