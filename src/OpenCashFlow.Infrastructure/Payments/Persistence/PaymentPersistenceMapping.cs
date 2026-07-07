using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Payments.Persistence;

internal static class PaymentPersistenceMapping
{
    public static PaymentSnapshot ToSnapshot(Payment payment)
    {
        return new PaymentSnapshot(
            payment.PaymentID,
            payment.TenantID,
            payment.RequestId,
            Convert.ToDecimal(payment.Amount),
            payment.EntryType,
            payment.PaymentMethodID,
            payment.DocumentTypeID,
            payment.UserID,
            payment.DateIns,
            payment.Description);
    }

    public static Payment ToEntity(PaymentDraft payment)
    {
        return new Payment
        {
            PaymentID = payment.PaymentId,
            TenantID = payment.TenantId,
            RequestId = payment.RequestId,
            Amount = Convert.ToDouble(payment.Amount),
            EntryType = payment.EntryType,
            PaymentMethodID = payment.PaymentMethodId,
            DocumentTypeID = payment.DocumentTypeId,
            UserID = payment.UserId,
            DateIns = payment.DateIns,
            Description = payment.Description
        };
    }

    public static void ApplyUpdate(Payment entity, PaymentUpdateDraft payment)
    {
        entity.Amount = Convert.ToDouble(payment.Amount);
        entity.PaymentMethodID = payment.PaymentMethodId;
        entity.DocumentTypeID = payment.DocumentTypeId;
        entity.Description = payment.Description;
        entity.EntryType = payment.EntryType;
        entity.DateIns = payment.DateIns;
        entity.DateEdit = DateTime.UtcNow;
        entity.EditedBy = payment.UserId;
    }
}
