using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCashFlow.Contracts.DTOs.Companies;
using System.Text.Json.Serialization;

namespace OpenCashFlow.Contracts.DTOs
{
    public class Company_Detail_DTO
    {
        public Guid TenantID { get; set; }

        #region Company Deatils
        public required string CompanyName { get; set; }

        public string? SysCompanyName { get; set; }

        public long MaxUsers { get; set; }

        public string? Avatar { get; set; }

        public string? BusinessCategory { get; set; }

        public decimal? EstimatedAnnualRevenue { get; set; }

        public string? BusinessHours { get; set; } // Esempio: "9:00 AM - 5:00 PM"

        public string? Website { get; set; }

        ///<summary>
        /// Esempio: JSON con link ai profili
        ///</summary>
        public string? SocialLinks { get; set; }

        public decimal? InternalRating { get; set; } // Esempio: 4.5 (valore da 0 a 5)

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
        /// lista di Indirizzi (se presente).
        /// </summary>
        public List<CompanyAddressDto>? CompanyAddresses { get; set; }

        /// <summary>
        /// lista di numeri di tel (se presente).
        /// </summary>
        public List<CompanyContactPhoneDto>? CompanyPhones { get; set; }

        /// <summary>
        /// lista di Indirizzi email (se presente).
        /// </summary>
        public List<CompanyContactEmailDto>? CompanyEmails { get; set; }

        #region Fiscal details
        public double? VATRates { get; set; }

        public string? VAT { get; set; }

        public string? SDI { get; set; }

        //todo: controllare nomenclatura correta ... NIN non e corretto
        public string? NIN { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TIN { get; set; }

        public string? AttorneyName { get; set; }

        public string? AttorneyMiddleName { get; set; }

        public string? AttorneySurname { get; set; }

        public string? AttorneyFullName
        {
            get =>
              ((AttorneySurname?.Trim() ?? "") + " " + (AttorneyMiddleName?.Trim() ?? "") + " " + (AttorneyName?.Trim() ?? "")).Trim();
        }

        //Todo: creare funzione per nome cognome e cognome nome
        #endregion

        #region Bank / Payment Details 
        public string? IBAN { get; set; }

        public string? BIC { get; set; }

        public string? SWIFT { get; set; }

        public string? PreferredPaymentMethod { get; set; }

        ///<summary>
        ///Per monitorare la spesa massima autorizzata dall'azienda.
        ///</summary>
        public decimal? MonthlyExpenseLimit { get; set; }

        public List<CompanyBillingAddressDto>? BillingDetails { get; set; }

        public double? BaseDiscountPercentage { get; set; }
        #endregion

        #region Contract details
        public DateTime StartingContract { get; set; } = DateTime.UtcNow;

        public DateTime EndingContract { get; set; }

        /// <summary>
        /// Informazioni sulla licenza in uso (se il tuo software ha diverse licenze). retail, 
        /// </summary>
        public string? LicenseType { get; set; }
        #endregion

        #region Privacy And Contract
        public bool GdprConsent { get; set; } = false;

        public DateTime? GdprConsentDate { get; set; }

        public bool ContractAcepted { get; set; } = false;

        public string? ContractVersion { get; set; }

        public DateTime? ContractAcceptedDate { get; set; }
        #endregion

        #region Default Values
        public string? DefaultCurrency { get; set; }

        public Guid? DefaultTimezoneID { get; set; }

        // New defaults for app auto-fill
        public string? DefaultTimezone { get; set; } // TimeZoneInfo.Id
        public string? DefaultLanguage { get; set; } // ISO 639-1 (e.g. "it")
        public string? DefaultCountry { get; set; }  // ISO 3166-1 alpha-2 (e.g. "IT")
        #endregion

        #region Security
        public bool IsActive { get; set; } = true;
        #endregion

        public Guid? StatusID { get; set; } = Guid.Empty; //todo: creare tabella da matchare ... il 00000 equivale ad attivo ...

        #region Deletititon
        public bool IsDeleted { get; set; } = false;

        public Guid? IsDeletedBy { get; set; }

        public string? IsDeletedWhy { get; set; }

        public DateTime? DateDeleted { get; set; }
        #endregion

        #region Audit & Tracking
        public Guid? CreatedBy { get; set; }

        public DateTime DateIns { get; set; } = DateTime.UtcNow;

        public Guid? EditedBy { get; set; }

        public DateTime? DateEdit { get; set; }
        #endregion
    }
}
