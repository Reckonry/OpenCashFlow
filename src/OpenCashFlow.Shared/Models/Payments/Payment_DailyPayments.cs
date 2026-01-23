using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models
{
    [Table("Payments_DailyPayments")]
    [PrimaryKey(nameof(DailyPaymentsID))]
    [Index(nameof(TenantID), nameof(CashDate), IsUnique = true)]
    public class Payment_DailyPayments
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column(Order = 0)]
        public Guid DailyPaymentsID { get; set; } = Guid.NewGuid();

        [Required, Column(Order = 1)]
        public Guid TenantID { get; set; }

        [Required, Column(TypeName = "date", Order = 2)]
        public DateTime CashDate { get; set; }

        [Required, Column(TypeName = "numeric(18,3)", Order = 3)]
        public double Total { get; set; } = 0.0;

        [Required, Column(Order = 901), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;
    }
}
