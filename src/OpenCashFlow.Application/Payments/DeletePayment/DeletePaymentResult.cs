namespace OpenCashFlow.Application.Payments.DeletePayment;

public sealed record DeletePaymentResult(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId);
