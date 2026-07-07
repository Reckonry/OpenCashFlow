using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Domain.Common;
using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Application.Payments.UpdatePayment;

public sealed class UpdatePaymentOrchestrator(
    IUpdatePaymentUseCase updatePaymentUseCase,
    IPaymentReader paymentReader,
    IPaymentWriter paymentWriter,
    IDailyPaymentWriter dailyPaymentWriter,
    ICashLedgerReader cashLedgerReader,
    ICashLedgerWriter cashLedgerWriter,
    IAuditWriter auditWriter,
    IUnitOfWork unitOfWork) : IUpdatePaymentOrchestrator
{
    private static readonly Guid SystemCashPaymentMethodId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly string[] CashMethodAliases = ["Cash", "Contanti"];

    public async Task<UpdatePaymentOrchestrationResult?> ExecuteAsync(UpdatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var validatedPayment = await updatePaymentUseCase.ExecuteAsync(command, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var originalPayment = await paymentReader.GetByIdAsync(
                validatedPayment.PaymentId,
                validatedPayment.TenantId,
                ct);

            if (originalPayment == null)
            {
                return null;
            }

            var newMethod = await paymentReader.GetPaymentMethodByIdAsync(
                validatedPayment.PaymentMethodId,
                validatedPayment.TenantId,
                ct);

            if (newMethod == null)
            {
                throw new ArgumentException("Payment method does not exist for the current tenant.", nameof(command.PaymentMethodId));
            }

            var documentType = await paymentReader.GetDocumentTypeByIdAsync(
                validatedPayment.DocumentTypeId,
                validatedPayment.TenantId,
                ct);

            if (documentType == null)
            {
                throw new ArgumentException("Document type does not exist for the current tenant.", nameof(command.DocumentTypeId));
            }

            var paymentDraft = new PaymentUpdateDraft(
                validatedPayment.PaymentId,
                validatedPayment.TenantId,
                validatedPayment.Amount,
                validatedPayment.NormalizedEntryType,
                validatedPayment.PaymentMethodId,
                validatedPayment.DocumentTypeId,
                validatedPayment.UserId,
                validatedPayment.DateIns,
                command.Description);

            var updatedPayment = await paymentWriter.UpdateAsync(paymentDraft, ct);
            if (updatedPayment == null)
            {
                return null;
            }

            await dailyPaymentWriter.DeleteDailyPaymentAsync(
                originalPayment.TenantId,
                originalPayment.DateIns,
                originalPayment.Amount,
                originalPayment.EntryType,
                ct);

            await dailyPaymentWriter.UpdateDailyPaymentAsync(
                updatedPayment.TenantId,
                updatedPayment.DateIns,
                updatedPayment.Amount,
                updatedPayment.EntryType,
                ct);

            var originalMethod = await paymentReader.GetPaymentMethodByIdAsync(
                originalPayment.PaymentMethodId,
                originalPayment.TenantId,
                ct);

            var cashLedgerApplied = await ApplyCashLedgerAsync(
                originalPayment,
                updatedPayment,
                originalMethod,
                newMethod,
                validatedPayment.CashDelta,
                validatedPayment.UserId,
                ct);

            await auditWriter.WritePaymentUpdatedAsync(
                new PaymentUpdateAudit(originalPayment, updatedPayment, validatedPayment.UserId),
                ct);

            return new UpdatePaymentOrchestrationResult(
                updatedPayment,
                originalPayment,
                validatedPayment,
                DailyPaymentApplied: true,
                CashLedgerApplied: cashLedgerApplied);
        }, cancellationToken);
    }

    private async Task<bool> ApplyCashLedgerAsync(
        PaymentSnapshot originalPayment,
        PaymentSnapshot updatedPayment,
        PaymentMethodSnapshot? originalMethod,
        PaymentMethodSnapshot? newMethod,
        decimal newDelta,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var wasOriginalCash = IsCashMethod(originalPayment.PaymentMethodId, originalMethod);
        var isNewCash = IsCashMethod(updatedPayment.PaymentMethodId, newMethod);

        if (!wasOriginalCash && !isNewCash)
        {
            return false;
        }

        var netBalance = await cashLedgerReader.GetPaymentNetBalanceAsync(
            updatedPayment.TenantId,
            updatedPayment.PaymentId,
            cancellationToken);

        var hasActiveCashEntry = netBalance != 0m;

        if (wasOriginalCash && !isNewCash)
        {
            await cashLedgerWriter.VoidPaymentAsync(
                updatedPayment.TenantId,
                updatedPayment.PaymentId,
                originalPayment.Amount,
                userId,
                cancellationToken);

            return true;
        }

        if (!wasOriginalCash && isNewCash)
        {
            if (hasActiveCashEntry)
            {
                await cashLedgerWriter.ApplyPaymentAsync(
                    updatedPayment.TenantId,
                    updatedPayment.PaymentId,
                    newDelta,
                    userId,
                    cancellationToken);
            }
            else if (await cashLedgerReader.HasVoidedPaymentAsync(updatedPayment.TenantId, updatedPayment.PaymentId, cancellationToken))
            {
                await cashLedgerWriter.ReapplyPaymentAsync(
                    updatedPayment.TenantId,
                    updatedPayment.PaymentId,
                    newDelta,
                    userId,
                    cancellationToken);
            }
            else
            {
                await cashLedgerWriter.ApplyPaymentAsync(
                    updatedPayment.TenantId,
                    updatedPayment.PaymentId,
                    newDelta,
                    userId,
                    cancellationToken);
            }

            return true;
        }

        var originalDelta = GetCashDelta(originalPayment.Amount, originalPayment.EntryType);
        if (hasActiveCashEntry)
        {
            await cashLedgerWriter.UpdatePaymentAsync(
                updatedPayment.TenantId,
                updatedPayment.PaymentId,
                originalDelta,
                newDelta,
                userId,
                cancellationToken);
        }
        else
        {
            await cashLedgerWriter.ReapplyPaymentAsync(
                updatedPayment.TenantId,
                updatedPayment.PaymentId,
                newDelta,
                userId,
                cancellationToken);
        }

        return true;
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

    private static decimal GetCashDelta(decimal amount, string entryType)
    {
        var paymentAmount = PaymentAmount.From(amount);
        var domainEntryType = string.Equals(entryType, "Outcome", StringComparison.OrdinalIgnoreCase)
            ? PaymentEntryType.Outflow
            : PaymentEntryType.Inflow;

        return PaymentRules.GetCashDelta(paymentAmount, domainEntryType);
    }
}
