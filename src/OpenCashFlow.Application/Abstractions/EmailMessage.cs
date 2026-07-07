namespace OpenCashFlow.Application.Abstractions
{
    public class EmailMessage
    {
        // Mittente
        public string? FromName { get; set; }

        // Contenuto
        public string Subject { get; set; }
        public string Content { get; set; }

        public EmailMessage(string subject, string content)
        {
            Subject = subject;
            Content = content;
        }
    }
}
