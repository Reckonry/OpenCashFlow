namespace OpenCashFlow.Application.Abstractions
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message, string DestUserName, string DestUserEmail);
        Task SendEmailAsync(EmailMessage message, string DestUserName, string DestUserEmail);
    }
}
