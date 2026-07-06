using OpenCashFlow.Application.Payments.Lookups;
using global::Shared.Models;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

internal static class PaymentMethodLookupMapping
{
    public static PaymentMethodListItem ToListItem(Payment_Method_LookUps method)
    {
        return new PaymentMethodListItem(
            method.PaymentMethodID,
            method.TenantID,
            method.PaymentMethodName,
            method.PaymentMethodDescription,
            method.PaymentMethodIcon,
            method.Visible,
            method.DisplayOrder,
            method.IsDeleted,
            method.IsDeletedBy,
            method.IsDeletedWhy,
            method.DateDeleted,
            method.CreatedBy,
            method.DateIns,
            method.EditedBy,
            method.DateEdit);
    }

    public static PaymentMethodResult ToResult(Payment_Method_LookUps method)
    {
        return new PaymentMethodResult(
            method.PaymentMethodID,
            method.TenantID,
            method.PaymentMethodName,
            method.PaymentMethodDescription,
            method.PaymentMethodIcon,
            method.Visible,
            method.DisplayOrder,
            method.IsDeleted,
            method.IsDeletedBy,
            method.IsDeletedWhy,
            method.DateDeleted,
            method.CreatedBy,
            method.DateIns,
            method.EditedBy,
            method.DateEdit);
    }
}
