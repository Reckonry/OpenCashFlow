using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class Payment_Filter_DTO
    {
        public Guid? PaymentID { get; set; }

        // Filtri principali
        public string? EntryType { get; set; }          // “Income” or “Outcome”
        public Guid? PaymentMethodID { get; set; }      // Cash/Card
        public Guid? DocumentTypeID { get; set; }       // Invoice/Receipt
        public Guid? UserID { get; set; }               // Dipendente
        public bool? IsDeleted { get; set; }            // Solo se eliminato

        // Range temporale
        public DateTime? FromDate { get; set; } = DateTime.Today;
        public DateTime? ToDate { get; set; }

        // Range importo (optional)
        public double? MinAmount { get; set; }
        public double? MaxAmount { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "DateIns";
        public bool Desc { get; set; } = true;
        public string? Description { get; set; }


        // Pagination
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 25;

        // Filtro per azienda
        public Guid? TenantID { get; set; }

        // Se true, non limitare i risultati all'utente corrente (mostra tutti gli utenti della company)
        public bool IncludeAllUsers { get; set; } = true;
    }
}
