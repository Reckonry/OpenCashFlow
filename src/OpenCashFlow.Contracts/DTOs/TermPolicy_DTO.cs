using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Contracts.DTOs
{
    public class TermPolicy_DTO
    {
        public string FallbackLanguage { get; set; } = default!;
        public string Title { get; set; } = string.Empty;
        public List<string> Description { get; set; } = [];
        public DateTime ValidFrom { get; set; }
        public List<TermPolicySection_DTO> Sections { get; set; } = new();
    }

    public class TermPolicySection_DTO
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = null!;
        public List<string> Content { get; set; } = [];
    }
}
