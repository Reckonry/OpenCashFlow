using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace OpenCashFlow.API.Controllers
{
#if DEBUG
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("dev/email")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DevEmailController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DevEmailController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // To preview: http://localhost:9000/dev/email/registration?lang=it&name=Lorenzo&uid=123&cid=456
        [HttpGet("registration")]
        public IActionResult RegistrationPreview([FromQuery] string name = "User", [FromQuery] string uid = "1", [FromQuery] string cid = "1")
        {
            if (!_env.IsDevelopment()) return NotFound();

            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "RegistrationConfirmation.html");
            if (!System.IO.File.Exists(templatePath))
                return NotFound($"Template not found: {templatePath}");

            var template = System.IO.File.ReadAllText(templatePath);
            var html = template
                .Replace("{{UserName}}", name)
                .Replace("{{VerificationLinkTenantID}}", cid)
                .Replace("{{VerificationLinkUserID}}", uid)
                .Replace("{{VerificationLink}}", $"test-{DateTime.Now.Ticks}");

            return Content(html, "text/html");
        }
    }
#endif
}
