namespace OpenCashFlow.Application.Payments.UpdatePayment;

public sealed record UpdatePaymentResult(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId,
    decimal Amount,
    string NormalizedEntryType,
    Guid PaymentMethodId,
    Guid DocumentTypeId,
    DateTime DateIns,
    decimal CashDelta);
