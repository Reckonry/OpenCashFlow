namespace OpenCashFlow.Application.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId,
    Guid RequestId,
    decimal Amount,
    string EntryType,
    Guid? PaymentMethodId,
    Guid? DocumentTypeId,
    DateTime DateIns,
    string? Description);
