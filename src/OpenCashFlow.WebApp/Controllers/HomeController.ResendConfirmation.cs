using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("ResendConfirmation"), Route("Account/ResendConfirmation")]
        public IActionResult ResendConfirmation(string? username)
        {
            ViewBag.Username = username ?? string.Empty;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ResendConfirmation"), Route("Account/ResendConfirmation")]
        public async Task<IActionResult> ResendConfirmationPost(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.ErrorMessage = _localizer["Please enter your email or username"];
                ViewBag.Username = string.Empty;
                return View("ResendConfirmation");
            }

            try
            {
                await _authAPIService.ResendConfirmationAsync(username);
                TempData["ResendConfirmationSuccess"] = true;
            }
            catch
            {
                // Even in case of errors, do not disclose details to the user
                TempData["ResendConfirmationSuccess"] = true;
            }

            return RedirectToAction("ResendConfirmation", new { username });
        }
    }
}

