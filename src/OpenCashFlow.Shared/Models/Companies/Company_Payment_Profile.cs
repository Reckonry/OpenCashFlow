using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    [Table(name: "Companies_Payments_Profiles")]
    [PrimaryKey(nameof(Payment_Profile_ID))]
    class Company_Payment_Profile
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Payment_Profile_ID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TenantID { get; set; }

        // Facoltativo: se vuoi tenere traccia di quale utente ha effettuato l’operazione
        public Guid? UserID { get; set; }

        [Required, MaxLength(50)]
        public required string Provider { get; set; }  // Es. "Stripe", "PayPal"

        [Required, MaxLength(100)]
        public required string ProviderCustomerId { get; set; }  // Es. "cus_ABC123..."

        [MaxLength(100), AllowNull]
        public string? ProviderSubscriptionId { get; set; }  // Es. ID dell’abbonamento, se applicabile

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "EUR";  // Valuta predefinita, es. "USD", "EUR"

        // Puoi usare una stringa per salvare dati JSON o, in alternativa, un oggetto serializzato
        [AllowNull]
        public string? Metadata { get; set; }

        // ID dell’utente che ha inserito i dati, per motivi di tracciamento e privacy

        #region Deletititon
        [Required, NotNull, Column(Order = 802), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 803), Display(Name = "Who deleted this Customer ?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 804), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 805), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        [AllowNull, Column(Order = 900), Display(Name = "Created By"), ForeignKey("UserID")]
        public Guid? CreatedBy { get; set; }

        [Required, NotNull, Column(Order = 901), DefaultValue("now()")]
        [Display(Name = "Created"), DataType(DataType.DateTime)]
        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        [AllowNull, Column(Order = 902), Display(Name = "Edited By"), ForeignKey("UserID")]
        public Guid? EditedBy { get; set; }

        [AllowNull, Column(Order = 903), Display(Name = "Date edit"), DataType(DataType.DateTime)]
        public DateTime? DateEdit { get; set; }
        #endregion
    }
}