namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Interface for the email template service
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Gets the HTML template for password reset with placeholders replaced
        /// </summary>
        Task<string> GetForgotPasswordTemplateAsync(string userName, string resetLink);
    }
}
