using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Companies_Subscriptions")]
    [PrimaryKey(nameof(SubscriptionID))]
    [Index(nameof(StripeSubscriptionID), IsUnique = true, Name = "IX_Companies_Subscriptions_StripeSubscriptionID")]
    [Index(nameof(StripeInvoiceID), Name = "IX_Companies_Subscriptions_StripeInvoiceID")]
    [Index(nameof(StripePriceID), Name = "IX_Companies_Subscriptions_StripePriceID")]
    public class Company_Subscription
    {
        #region Data Linking
        [Required, NotNull, Key, Column(Order = 1)]
        public Guid TenantID { get; set; }
        #endregion

        [Required, NotNull, Key, Column(Order = 10), DefaultValue("gen_random_uuid()"),
            DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SubscriptionID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 11)]
        /// <summary>Associated plan ID.</summary>
        public Guid PlanID { get; set; }

        [Required, NotNull, Column(Order = 12, TypeName = "timestamp"), DefaultValue("now()")]
        /// <summary>
        /// Subscription start date.
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required, NotNull, Column(Order = 13, TypeName = "timestamp")]
        /// <summary>Subscription end date (calculated based on renewal frequency).</summary>
        public DateTime EndDate { get; set; }

        [Required, NotNull, Column(Order = 14, TypeName = "timestamp")]
        /// <summary>Next scheduled payment date.</summary>
        public required DateTime NextBillingDate { get; set; }

        [Required, NotNull, Column(Order = 15, TypeName = "varchar(50)")]
        /// <summary>
        /// Billing cycle (e.g., "MONTHLY", "YEARLY").
        /// </summary>
        public required BillingCycle BillingCycle { get; set; }

        [Required, NotNull, Column(Order = 16)]
        /// <summary>
        /// Specifies the day of the month when payment is due.
        /// </summary>
        public required int BillingDay { get; set; }

        [Required, NotNull, Column(Order = 17, TypeName = "varchar(50)")]
        /// <summary> Subscription status (e.g., ACTIVE, PAUSED, EXPIRED, CANCELLED).</summary>
        public required RenewalStatus RenewalStatus { get; set; }

        [Required, NotNull, Column(Order = 18, TypeName = "timestamp")]
        /// <summary>Date of the last payment reminder sent.</summary>
        public required DateTime LastReminderDate { get; set; }

        [Required, NotNull, Column(Order = 19, TypeName = "timestamp")]
        /// <summary>Date of the next payment reminder.</summary>
        public required DateTime NextReminderDate { get; set; }

        [Required, NotNull, Column(Order = 20, TypeName = "NUMERIC(18,3)")]
        /// <summary>Cost specific to this company.</summary>
        public required double Cost { get; set; }

        [AllowNull, Column(Order = 21, TypeName = "NUMERIC(18,3)")]
        /// <summary>Discount percentage applied.</summary>
        public double? Discount { get; set; }

        [AllowNull, Column(Order = 22)]
        /// <summary>Promo code used to get the discount.</summary>
        public string? PromoCode { get; set; }

        [AllowNull, Column(Order = 23, TypeName = "timestamp")]
        /// <summary>Discount expiration date, if temporary.</summary>
        public DateTime? DiscountExpiration { get; set; }

        [AllowNull, Column(Order = 24, TypeName = "timestamp")]
        /// <summary>Date when the subscription was cancelled.</summary>
        public DateTime? CancellationDate { get; set; }

        [AllowNull, Column(Order = 25, TypeName = "text")]
        /// <summary>Cancellation reason.</summary>
        public string? CancellationReason { get; set; }

        #region Stripe Integration
        [AllowNull, Column(Order = 26, TypeName = "varchar(100)"), Display(Name = "Stripe Subscription ID")]
        /// <summary>ID of the associated Stripe subscription.</summary>
        public string? StripeSubscriptionID { get; set; }

        [AllowNull, Column(Order = 27, TypeName = "varchar(100)"), Display(Name = "Stripe Price ID")]
        /// <summary>ID of the Stripe price used for this subscription.</summary>
        public string? StripePriceID { get; set; }

        [AllowNull, Column(Order = 28, TypeName = "varchar(100)"), Display(Name = "Stripe Invoice ID")]
        /// <summary>ID of the latest generated Stripe invoice.</summary>
        public string? StripeInvoiceID { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 902), Display(Name = "Edited By")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Column(Order = 903)]
        [Display(Name = "Material date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion


        [AllowNull, ForeignKey(nameof(PlanID))]
        public virtual Plan? Plan { get; set; }

        [AllowNull, ForeignKey(nameof(TenantID))]
        public virtual Company? Company { get; set; }

    }


   // nextBillingDate'; 'billingCycle'; 'billingDay'; 'renewalStatus'.'

    public enum BillingCycle
    {
        MONTHLY,
        YEARLY
    }
}
