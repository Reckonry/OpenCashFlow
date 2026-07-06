using global::Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.App.Services;

namespace OpenCashFlow.App.Controllers
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
                AdminPassword = string.Empty,
                ConfirmPassword = string.Empty,
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

            var result = await _setupAPIService.CompleteSetupAsync(model, cancellationToken);
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.Message ?? "Unable to complete setup.";
                return View(model);
            }

            TempData["SetupCompleted"] = "Setup completed. Sign in with the administrator account.";
            return RedirectToAction(nameof(Login));
        }
    }
}
