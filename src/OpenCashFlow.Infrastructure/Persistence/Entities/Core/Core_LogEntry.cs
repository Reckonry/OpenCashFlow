using System.ComponentModel.DataAnnotations;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Core
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
