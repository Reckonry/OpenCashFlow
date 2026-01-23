using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Cash
{
    [Table("CashLedgers")]
    public class CashLedger
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CompanyId { get; set; }

        [Required]
        [MaxLength(32)]
        public string RefType { get; set; } = default!; // "Payment","Refund","Void","Adjustment"

        public Guid RefId { get; set; }

        /// <summary>
        /// Tracks the original payment ID for related entries (Void, PaymentReapply, PaymentUpdate).
        /// Allows querying all entries related to a specific payment regardless of RefId.
        /// </summary>
        public Guid? OriginalPaymentId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Delta { get; set; }

        public string? Reason { get; set; }

        [Required]
        public string CreatedBy { get; set; } = default!;

        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}

