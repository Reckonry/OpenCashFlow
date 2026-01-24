using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    public class BillingPortalSessionRequest_DTO
    {
        [Required, Url]
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
