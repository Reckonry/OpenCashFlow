using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Stripe
{
    [Table("Stripe_Webhook_Events")]
    public class Stripe_Webhook_Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string EventId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string EventType { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedUtc { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ReceivedUtc { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? ProcessedUtc { get; set; }

        [Required]
        public bool Success { get; set; }

        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public required string Payload { get; set; }
    }
}
