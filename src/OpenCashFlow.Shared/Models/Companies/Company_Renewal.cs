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

namespace Shared.Models
{
    [Table(name: "Companies_Renewals")]
    [PrimaryKey(nameof(RenewalID))]
    public class Company_Renewal
    {
        #region Data Linking
        [Required, NotNull, Key, Column(Order = 1)]
        public Guid TenantID { get; set; }
        #endregion

        [Required, NotNull, Key, Column(Order = 10), DefaultValue("gen_random_uuid()")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RenewalID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(Order = 11)]
        /// <summary>Per indicare il piano attivo al momento del rinnovo</summary>
        public Guid PlanID { get; set; }

        [Required, NotNull, Column(Order = 12)]
        /// <summary>ID dell'abbonamento a cui si riferisce questo rinnovo.</summary>
        public Guid SubscriptionID { get; set; }

        [AllowNull, Column(TypeName = "timestamp", Order = 13)]
        /// <summary>Data del pagamento effettuato.</summary>
        public DateTime? BillingDate { get; set; }

        [Required, NotNull, Column(TypeName = "numeric(18,3)", Order = 14)]
        /// <summary>Importo pagato per il rinnovo.</summary>
        public double AmountPaid { get; set; }

        [Required, NotNull, Column(TypeName = "numeric(18,3)", Order = 15)]
        /// <summary> Campo JSON con i dettagli delle tasse applicate(es.VAT, IVA, ecc.).</summary>
        public double TaxDetails { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(50)", Order = 16)]
        /// <summary>La valuta utilizzata per il pagamento (es. "USD", "EUR").</summary>
        public required string Currency { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(150)", Order = 17)]
        /// <summary>Metodo di pagamento utilizzato (es. "Credit Card"). </summary>
        public required PaymentMethod PaymentMethod { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(150)", Order = 18)]
        /// <summary>Stato del rinnovo (es. "PAID", "FAILED")</summary>
        public required RenewalStatus RenewalStatus { get; set; }

        [Required, NotNull, Column(TypeName = "varchar(150)", Order = 19), DefaultValue(0)]
        /// <summary>(Optional) Penalty applied for late payments.</summary>
        public double LateFee { get; set; } = 0;

        [AllowNull, Column(TypeName = "varchar(150)", Order = 20)]
        /// <summary>Collegamento alla fattura generata per questo rinnovo</summary>
        public Guid InvoiceID { get; set; }

        [Required, Column(TypeName = "varchar(150)", Order = 21)]
        /// <summary>Il processore utilizzato per il pagamento (es. "Stripe", "PayPal", "Bank Transfer").</summary>
        public required string PaymentProcessor { get; set; }

        [AllowNull, Column(TypeName = "varchar(150)", Order = 22)]
        /// <summary>ID univoco della transazione restituito dal processore di pagamento</summary>
        public string? TransactionID { get; set; }

        [AllowNull, Column(TypeName = "varchar(150)", Order = 23)]
        /// <summary>Stato della transazione (es. "SUCCESS", "PENDING", "FAILED").</summary>
        public string? TransactionStatus { get; set; }

        [AllowNull, Column(TypeName = "varchar(150)", Order = 24)]
        /// <summary>Specifica il tipo di rinnovo (es. "AUTOMATIC", "MANUAL")</summary>
        public RenewalType? RenewalType { get; set; } = Models.RenewalType.AUTOMATIC;

        [AllowNull, Column(TypeName = "Text", Order = 25)]
        /// <summary>Per aggiungere dettagli o note relativi al rinnovo, ad esempio spiegare un ritardo o un problema.</summary>
        public string? Comments { get; set; }

        [ForeignKey(nameof(PlanID))]
        public virtual Plan? Plan { get; set; }

        [ForeignKey(nameof(SubscriptionID))]
        public virtual Company_Subscription? Subscription { get; set; }
    }

    public enum RenewalStatus
    {
        ACTIVE,
        PAUSED,
        EXPIRED,
        CANCELLED,
        TRIAL,
        SUSPENDED
    }

    public enum RenewalType
    {
        AUTOMATIC,
        MANUAL
    }

    public enum PaymentMethod
    {
        CREDIT_CARD,
        BANK_TRANSFER,
        PAYPAL,
        STRIPE,
        OTHER
    }
}
