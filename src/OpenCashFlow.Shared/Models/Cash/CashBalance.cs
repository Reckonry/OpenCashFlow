using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Cash
{
    [Table("CashBalances")]
    public class CashBalance
    {
        [Key]
        public Guid CompanyId { get; set; }
         
        [Column(TypeName = "numeric(18,2)")]
        public decimal Balance { get; set; }

        public DateTimeOffset LastUpdatedUtc { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}

