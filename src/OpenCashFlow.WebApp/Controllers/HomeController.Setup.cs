using OpenCashFlow.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.WebApp.Models.Setup;
using OpenCashFlow.WebApp.Services;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController
    {
        private readonly SetupAPIService _setupAPIService;

        [HttpGet]
        [Route("Setup")]
        public async Task<IActionResult> Setup(CancellationToken cancellationToken)
        {
            var status = await _setupAPIService.GetStatusAsync(cancellationToken);
            if (status is { RequiresSetup: false })
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new SetupRequest_DTO
            {
                CompanyName = string.Empty,
                AdminEmail = string.Empty,
                AdminFirstName = string.Empty,
                Language = "it",
                Currency = "EUR",
                Timezone = "Europe/Rome",
                Country = "IT"
            });
        }

        [HttpPost]
        [Route("Setup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup([Bind] SetupRequest_DTO model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.AdminPassword = null;
            model.ConfirmPassword = null;

            var result = await _setupAPIService.CompleteSetupAsync(model, cancellationToken);
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.Message ?? "Unable to complete setup.";
                return View(model);
            }

            if (result.Setup is null || string.IsNullOrWhiteSpace(result.Setup.TemporaryAdminPassword))
            {
                ViewBag.ErrorMessage = "Setup completed but the temporary password was not returned. Reset the admin password before signing in.";
                return View(model);
            }

            return View("SetupComplete", new SetupCompleteViewModel
            {
                AdminEmail = result.Setup.AdminEmail,
                TemporaryAdminPassword = result.Setup.TemporaryAdminPassword
            });
        }
    }
}
