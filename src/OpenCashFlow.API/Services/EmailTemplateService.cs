using OpenCashFlow.API.Services.Interfaces;

namespace OpenCashFlow.API.Services
{
    /// <summary>
    /// Service to manage HTML email templates
    /// Loads templates from the EmailTemplates folder and replaces placeholders
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Loads the HTML template for password reset and replaces placeholders
        /// </summary>
        /// <param name="userName">User name to personalize the email</param>
        /// <param name="resetLink">Password reset link</param>
        /// <returns>Email HTML with placeholders replaced</returns>
        public async Task<string> GetForgotPasswordTemplateAsync(string userName, string resetLink)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "ForgotPassword.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template non trovato: {templatePath}");
            }

            var template = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders with actual values
            return template
                .Replace("{{UserName}}", userName)
                .Replace("{{ResetLink}}", resetLink);
        }
    }
}
