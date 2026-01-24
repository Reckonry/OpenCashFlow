using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    public class BillingAddPaymentMethodRequest_DTO
    {
        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;

        public bool MakeDefault { get; set; } = true;
    }
}
