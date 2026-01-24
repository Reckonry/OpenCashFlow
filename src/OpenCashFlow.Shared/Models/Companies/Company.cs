using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models
{
    [Table(name: "Companies")]
    [PrimaryKey(nameof(TenantID))]
    [Index(nameof(StripeCustomerID), IsUnique = true, Name = "IX_Companies_StripeCustomerID")]
    [Index(nameof(StripeDefaultPaymentMethodID), IsUnique = true, Name = "IX_Companies_StripeDefaultPaymentMethodID")]
    [Index(nameof(BillingEmail), Name = "IX_Companies_BillingEmail")]
    public class Company
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TenantID { get; set; } = Guid.NewGuid();

        #region Company Deatils
        [Required(ErrorMessage = "Provide a company name"), NotNull, Column(TypeName = "varchar(256)", Order = 10), Display(Name = "Company Name")]
        public required string CompanyName { get; set; } 

        [NotMapped, Display(Name = "System Company Name")]
        public string? SysCompanyName { get; set; }

        [Required(ErrorMessage = "Set maximum user amount"), NotNull, Column(Order = 11)]
        public long MaxUsers { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 12), Display(Name = "Avatar")]
        public string? Avatar { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 13), Display(Name = "Business Category")]
        public string? BusinessCategory { get; set; }

        [AllowNull, Column(TypeName = "decimal(18,3)", Order = 14), Display(Name = "Estimated Annual Revenue")]
        public decimal? EstimatedAnnualRevenue { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 15), Display(Name = "Business Hours")]
        public string? BusinessHours { get; set; } // Esempio: "9:00 AM - 5:00 PM"

        [AllowNull, Column(TypeName = "varchar(256)", Order = 16), Display(Name = "Company Website")]
        public string? Website { get; set; }

        [AllowNull, Column(TypeName = "varchar(512)", Order = 17), Display(Name = "Social Media Links")]
        ///<summary>
        /// Esempio: JSON con link ai profili
        ///</summary>
        public string? SocialLinks { get; set; }

        [AllowNull, Column(TypeName = "decimal(3,2)", Order = 18), Display(Name = "Internal Rating")]
        public decimal? InternalRating { get; set; } // Esempio: 4.5 (valore da 0 a 5)

        [Required, Column(Order = 19), Display(Name = "Priority Level")]
        ///<summary>
        /// prority level of the company (support) ?
        ///</summary>
        ///<value>
        ///<!-- 0 = Low, 1 = Medium, 2 = high -->
        ///</value>
        public int PriorityLevel { get; set; } = 0;
        #endregion


        //todo: Add navigation properties for the following:
        /// <summary>
        /// Navigazione alla lista di Indirizzi (se presente).
        /// </summary>
        public virtual List<Company_Address>? CompanyAddresses { get; set; }

        /// <summary>
        /// Navigazione alla lista di numeri di tel (se presente).
        /// </summary>
        public virtual List<Company_Contact_Phone>? CompanyPhones { get; set; }

        /// <summary>
        /// Navigazione alla lista di Indirizzi email (se presente).
        /// </summary>
        public virtual List<Company_Contact_Email>? CompanyEmails { get; set; }

        #region Fiscal details
        [AllowNull, Column(TypeName = "numeric(18,3)", Order = 30), Display(Name = "Applicable VAT Rates")]
        public double? VATRates { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 31), Display(Name = "VAT")]
        public string? VAT { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 32), Display(Name = "SDI")]
        public string? SDI { get; set; }

        //todo: controllare nomenclatura correta ... NIN non e corretto
        [AllowNull, Column(TypeName = "varchar(256)", Order = 33), Display(Name = "Tax Identification Number")]
        public string? TIN { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 34), Display(Name = "Attorney Name")]
        public string? AttorneyName { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 35), Display(Name = "Attorney Middle Name")]
        public string? AttorneyMiddleName { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 36), Display(Name = "Attorney Surname")]
        public string? AttorneySurname { get; set; }

        [NotMapped]
        public string? AttorneyFullName
        {
            get =>
              ((AttorneySurname?.Trim() ?? "") + " " + (AttorneyMiddleName?.Trim() ?? "") + " " + (AttorneyName?.Trim() ?? "")).Trim();
        }

        //Todo: creare funzione per nome cognome e cognome nome
        #endregion

        #region Bank / Payment Details 
        [AllowNull, Column(TypeName = "varchar(256)", Order = 40), Display(Name = "IBAN")]
        public string? IBAN { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 41), Display(Name = "BIC")]
        public string? BIC { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 42), Display(Name = "SWIFT")]
        public string? SWIFT { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 43), Display(Name = "Preferred Payment Method")]
        public string? PreferredPaymentMethod { get; set; }

        [AllowNull, Column(TypeName = "decimal(18,2)", Order = 44), Display(Name = "Monthly Expense Limit")]
        ///<summary>
        ///Per monitorare la spesa massima autorizzata dall'azienda.
        ///</summary>
        public decimal? MonthlyExpenseLimit { get; set; }

        public virtual List<Company_BillingAddress>? BillingDetails { get; set; }

        [AllowNull, Column(TypeName = "numeric(18, 3)", Order = 45), Display(Name = "Base Discount Percentage")]
        public double? BaseDiscountPercentage { get; set; }

        #region Stripe Integration
        [AllowNull, Column(TypeName = "varchar(100)", Order = 46), Display(Name = "Stripe Customer ID")]
        /// <summary>ID del customer Stripe associato a questa azienda</summary>
        public string? StripeCustomerID { get; set; }

        [AllowNull, Column(TypeName = "varchar(100)", Order = 47), Display(Name = "Stripe Default Payment Method ID")]
        /// <summary>ID del metodo di pagamento di default in Stripe</summary>
        public string? StripeDefaultPaymentMethodID { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 48), Display(Name = "Billing Email")]
        /// <summary>Dedicated billing email (can be different from contact emails).</summary>
        public string? BillingEmail { get; set; }
        #endregion
        #endregion

        #region Contract details
        [Required(ErrorMessage = "Provide a contract start date"), NotNull, Column(Order = 50)]
        [Display(Name = "Contract date start"), DataType(DataType.DateTime)]
        public DateTime StartingContract { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Provide a contract expiration date"), NotNull, Column(Order = 51)]
        [Display(Name = "Contract date Expiration"), DataType(DataType.DateTime)]
        public DateTime EndingContract { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 52), Display(Name = "License Type")]
        /// <summary>
        /// Informazioni sulla licenza in uso (se il tuo software ha diverse licenze). retail, 
        /// </summary>
        public string? LicenseType { get; set; }
        #endregion

        #region Privacy And Contract
        [Required, Column(Order = 60), Display(Name = "GDPR Consent")]
        public bool GdprConsent { get; set; } = false;

        [AllowNull, Column(Order = 61), Display(Name = "GDPR Consent Date"), DataType(DataType.DateTime)]
        public DateTime? GdprConsentDate { get; set; }

        [Required, Column(Order = 62), Display(Name = "Contract Acepted"), DefaultValue(false)]
        public bool ContractAcepted { get; set; } = false;

        [AllowNull, Column(Order = 63), Display(Name = "Contract Version")]
        public string? ContractVersion { get; set; }

        [AllowNull, Column(Order = 64), Display(Name = "Contract Accepted Date")]
        public DateTime? ContractAcceptedDate { get; set; }
        #endregion

        #region Default Values
        [AllowNull, Column(TypeName = "varchar(256)", Order = 70), Display(Name = "Default Currency")]
        public string? DefaultCurrency { get; set; }

        [AllowNull, Column(Order = 71), Display(Name = "Default timezone"), ForeignKey("Timezone")]
        public string? DefaultTimezone { get; set; }

        [AllowNull, Column(TypeName = "varchar(5)", Order = 72), Display(Name = "Default Language (ISO 639-1)")]
        public string? DefaultLanguage { get; set; }

        [AllowNull, Column(TypeName = "varchar(2)", Order = 73), Display(Name = "Default Country (ISO 3166-1 alpha-2)")]
        public string? DefaultCountry { get; set; }
        #endregion

        #region Security
        [AllowNull, Column(TypeName = "text", Order = 80), Display(Name = "Company Secret")]
        ///<summary>Used for Quick PIN logins</summary>
        public required string CompanySecret { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 81), Display(Name = "Mobile Pin")]
        public string? MobilePin { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 82), Display(Name = "Master Password")]
        public string? MasterPassword { get; set; }

        [Required, NotNull, Column(Order = 83), Display(Name = "Is Company Active ?")]
        public bool IsActive { get; set; } = true;
        #endregion

        [AllowNull, Column(Order = 700), Display(Name = "StatusID"), ForeignKey("StatusID")]
        public Guid? StatusID { get; set; } = Guid.Empty; //todo: creare tabella da matchare ... il 00000 equivale ad attivo ...

        #region Deletititon
        [Required, NotNull, Column(Order = 800), Display(Name = "Is Customer deleted ?"), DefaultValue(false)]
        public bool IsDeleted { get; set; } = false;

        [AllowNull, Column(Order = 801), Display(Name = "Who deleted this Customer ?"), ForeignKey("UserID")]
        public Guid? IsDeletedBy { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 802), Display(Name = "Why is Customer deleted ?")]
        public string? IsDeletedWhy { get; set; }

        [AllowNull, Column(Order = 803), Display(Name = "Date deleted"), DataType(DataType.DateTime)]
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

        [AllowNull]
        public virtual Company_Subscription? Company_Subscription { get; set; }

    }
}
