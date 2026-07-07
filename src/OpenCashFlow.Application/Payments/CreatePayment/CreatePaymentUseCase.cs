using OpenCashFlow.Domain.Common;
using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Application.Payments.CreatePayment;

public sealed class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private const string IncomeEntryType = "Income";
    private const string OutcomeEntryType = "Outcome";

    public Task<CreatePaymentResult> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(command);

        var paymentId = PaymentId.From(command.PaymentId);
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);
        var amount = PaymentAmount.From(command.Amount);
        var (normalizedEntryType, domainEntryType) = NormalizeEntryType(command.EntryType);
        var paymentMethodId = RequireGuid(command.PaymentMethodId, "Payment method is required.", nameof(command.PaymentMethodId));
        var documentTypeId = RequireGuid(command.DocumentTypeId, "Document type is required.", nameof(command.DocumentTypeId));
        var requestId = RequireGuid(command.RequestId, "Request id is required.", nameof(command.RequestId));

        if (command.DateIns == default)
        {
            throw new ArgumentException("Payment date is required.", nameof(command.DateIns));
        }

        var normalizedDate = NormalizeDate(command.DateIns);
        var cashDelta = PaymentRules.GetCashDelta(amount, domainEntryType);

        var result = new CreatePaymentResult(
            paymentId.Value,
            tenantId.Value,
            userId.Value,
            requestId,
            amount.Amount,
            normalizedEntryType,
            paymentMethodId,
            documentTypeId,
            normalizedDate,
            cashDelta);

        return Task.FromResult(result);
    }

    private static (string NormalizedEntryType, PaymentEntryType DomainEntryType) NormalizeEntryType(string? entryType)
    {
        if (string.Equals(entryType, IncomeEntryType, StringComparison.OrdinalIgnoreCase))
        {
            return (IncomeEntryType, PaymentEntryType.Inflow);
        }

        if (string.Equals(entryType, OutcomeEntryType, StringComparison.OrdinalIgnoreCase))
        {
            return (OutcomeEntryType, PaymentEntryType.Outflow);
        }

        throw new ArgumentException("Entry type must be Income or Outcome.", nameof(entryType));
    }

    private static Guid RequireGuid(Guid? value, string message, string paramName)
    {
        if (!value.HasValue || value.Value == Guid.Empty)
        {
            throw new ArgumentException(message, paramName);
        }

        return value.Value;
    }

    private static Guid RequireGuid(Guid value, string message, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message, paramName);
        }

        return value;
    }

    private static DateTime NormalizeDate(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
