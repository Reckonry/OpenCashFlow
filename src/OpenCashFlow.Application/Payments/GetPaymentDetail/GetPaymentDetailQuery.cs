namespace OpenCashFlow.Application.Payments.GetPaymentDetail;

public sealed record GetPaymentDetailQuery(Guid PaymentId, Guid TenantId);
