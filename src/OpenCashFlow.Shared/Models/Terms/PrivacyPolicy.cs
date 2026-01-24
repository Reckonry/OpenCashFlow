using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Models.Terms
{

    public class TermPolicy
    {
        [JsonPropertyName("fallbackLanguage")]
        public string FallbackLanguage { get; set; } = default!;

        [JsonPropertyName("title")]
        public LocalizedString Title { get; set; } = [];

        [JsonPropertyName("description")]
        public LocalizedList Description { get; set; } = [];

        [JsonPropertyName("validFrom")]
        public DateTime ValidFrom { get; set; }

        [JsonPropertyName("sections")]
        public List<TermPolicySection> Sections { get; set; } = [];
    }

    public class LocalizedString : Dictionary<string, string>
    {
        // Mappa "it"->string, "en"->string, ecc.
    }

    public class LocalizedList : Dictionary<string, List<string>>
    {
        // Mappa "it"->List<string>, "en"->List<string>, ecc.
    }

    public class TermPolicySection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("title")]
        public LocalizedString Title { get; set; } = [];

        [JsonPropertyName("content")]
        public LocalizedList Content { get; set; } = [];
    }
}
