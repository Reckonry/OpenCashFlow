namespace OpenCashFlow.Application.Payments.DeletePayment;

public sealed record DeletePaymentCommand(
    Guid PaymentId,
    Guid TenantId,
    Guid UserId);
