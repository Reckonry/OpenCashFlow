using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    /// <summary>
    /// DTO to represent an event in the payments calendar
    /// Compatible with FullCalendar.js
    /// </summary>
    public class Payment_CalendarEvent_DTO
    {
        /// <summary>
        /// Unique event ID (format: "payment-{date}")
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Event title to display in the calendar
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Event date (payment DateIns)
        /// </summary>
        [Required]
        public DateTime Start { get; set; }

        /// <summary>
        /// Whether the event lasts all day
        /// </summary>
        public bool AllDay { get; set; } = true;

        /// <summary>
        /// URL to open on click (optional)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Extended properties for FullCalendar
        /// </summary>
        public PaymentEventExtendedProps ExtendedProps { get; set; } = new();
    }

    /// <summary>
    /// Extended properties for the calendar event
    /// </summary>
    public class PaymentEventExtendedProps
    {
        /// <summary>
        /// Calendar category (for different colors)
        /// </summary>
        public string Calendar { get; set; } = "payments";

        /// <summary>
        /// Number of payments on that date
        /// </summary>
        public int PaymentCount { get; set; }

        /// <summary>
        /// Total amount of payments on that date
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Date in YYYY-MM-DD format for filtering
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Predominant entry type (Income/Outcome)
        /// </summary>
        public string? EntryType { get; set; }
    }
}
