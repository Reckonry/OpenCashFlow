using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Plans_Prices")]
    [PrimaryKey(nameof(PriceID))]
    public class Plan_Price
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), DefaultValue("gen_random_uuid()")]
        public Guid PriceID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 1)]
        public Guid PlanID { get; set; } // FK to Plans

        [Required, NotNull, Column(TypeName = "varchar(50)", Order = 10), Display(Name = "Currency")]
        /// <summary>
        /// Default currency is EUR.
        /// </summary>
        public string Currency { get; set; } = "EUR";

        [Required, NotNull, Column(TypeName = "numeric(18,3)", Order = 11), Display(Name = "Amount")]
        public decimal Amount { get; set; } // Price amount

        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 12), Display(Name = "Is Default")]
        public bool IsDefault { get; set; } = false; // Whether this is the default price


        [AllowNull, Column(TypeName = "timestamp", Order = 20), ForeignKey("StartDate")]
        /// <summary>
        /// Start of the period when this plan can be purchased
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(TypeName = "timestamp", Order = 21), ForeignKey("EndDate")]
        /// <summary>
        /// End of the period when this plan can be purchased
        /// </summary>
        public DateTime? EndDate { get; set; }

        [Required, NotNull, Column(TypeName = "numeric(5,3)", Order = 22), Display(Name = "Discount Percentage")]
        /// <summary>
        /// Discount percentage for this plan with the promo code
        /// </summary>
        public decimal DiscountPercentage { get; set; } = 0;

        [Required, NotNull, Column(TypeName = "varchar(100)", Order = 23), Display(Name = "Promotion Code")]
        /// <summary>
        /// Promo code
        /// </summary>
        public string? PromotionCode { get; set; }

        [Required, NotNull, Column(TypeName = "timestamp", Order = 30), DefaultValue("now()"), Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Creation date

    }
}

/*
 PriceID	PlanID	Currency	Amount	IsDefault	CreatedAt
    1	1	USD	19.99	TRUE	2024-12-18 10:00:00
    2	1	EUR	18.50	FALSE	2024-12-18 10:00:00
    3	1	GBP	16.99	FALSE	2024-12-18 10:00:00
 */
