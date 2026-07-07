namespace OpenCashFlow.Infrastructure.Email
{
    public class EmailOption
    {
        public required string From { get; set; }
        public string FromName { get; set; } = "OpenCashFlow";
        public required string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
