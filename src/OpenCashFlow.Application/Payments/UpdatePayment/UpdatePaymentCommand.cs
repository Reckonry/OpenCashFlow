namespace OpenCashFlow.Application.Payments.UpdatePayment;

public sealed record UpdatePaymentCommand(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId,
    decimal Amount,
    string? EntryType,
    Guid? PaymentMethodId,
    Guid? DocumentTypeId,
    DateTime DateIns,
    string? Description);
