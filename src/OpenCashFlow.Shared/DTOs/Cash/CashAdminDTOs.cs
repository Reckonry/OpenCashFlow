using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Cash
{
    public class CashAdjustRequest
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public decimal Delta { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class CashRebuildRequest
    {
        [Required]
        public Guid CompanyId { get; set; }
    }
}

