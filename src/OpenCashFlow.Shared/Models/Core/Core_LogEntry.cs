using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Core
{
    public class Core_LogEntry
    {
        public int Id { get; set; }

        [Required]
        public required string ApiKey { get; set; }

        [Required]
        public required string Application { get; set; }

        public required string Module { get; set; }

        [Required]
        public required string LogType { get; set; } // error, warning, info, security

        [Required]
        public required string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
