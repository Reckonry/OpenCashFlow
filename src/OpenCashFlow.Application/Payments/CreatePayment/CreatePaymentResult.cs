namespace OpenCashFlow.Application.Payments.CreatePayment;

public sealed record CreatePaymentResult(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId,
    Guid RequestId,
    decimal Amount,
    string NormalizedEntryType,
    Guid PaymentMethodId,
    Guid DocumentTypeId,
    DateTime DateIns,
    decimal CashDelta);
