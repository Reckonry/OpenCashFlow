using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("ChangePassword")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        public class ChangePasswordViewModel
        {
            public string? NewPassword { get; set; }
            public string? RepeatPassword { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.RepeatPassword))
            {
                ViewBag.ErrorMessage = _localizer["Please fill all fields"];
                return View(model);
            }
            if (model.NewPassword != model.RepeatPassword)
            {
                ViewBag.ErrorMessage = _localizer["Passwords do not match"];
                return View(model);
            }

            try
            {
                var resp = await _authAPIService.ChangePasswordRequiredAsync(model.NewPassword);
                if (!resp.Success)
                {
                    ViewBag.ErrorMessage = resp.Message ?? _localizer["Unable to change password"];
                    return View(model);
                }

                // Delete auth cookies to force re-login with new password
                HttpContext.Response.Cookies.Delete(global::Shared.Core.Configuration.AuthCookieName,
                    new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });
                HttpContext.Response.Cookies.Delete(global::Shared.Core.Configuration.AuthCookieName + ".Info",
                    new CookieOptions { Domain = _configuration["Account:CookieDomain"], Path = "/" });

                // Use Login view localizer for the message
                var loginLocalizer = _localizerFactory.Create("Views.Home.Login", typeof(HomeController).Assembly.GetName().Name!);
                TempData["PasswordChangeSuccessMessage"] = loginLocalizer["PasswordUpdatedLoginAgain"].Value;
                return Redirect(_configuration["Account:Login"]!);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("KeepCurrentPassword")]
        public async Task<IActionResult> KeepCurrentPassword()
        {
            try
            {
                var resp = await _authAPIService.KeepCurrentPasswordAsync();
                if (!resp.Success)
                {
                    ViewBag.ErrorMessage = resp.Message ?? _localizer["Unable to keep current password"];
                    return View("ChangePassword");
                }

                // Redirect to app without forcing re-login
                return Redirect(_configuration["Account:AppUrl"]!);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("ChangePassword");
            }
        }
    }
}
