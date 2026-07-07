using OpenCashFlow.Contracts.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;
using OpenCashFlow.WebApp.Models;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("Reset-Password"), Route("Account/Reset-Password")]
        [Route("ResetPassword"), Route("Account/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string? token)
        {
            _logger.LogInformation("ResetPassword called with token: '{Token}' (Length: {Length})", token ?? "NULL", token?.Length ?? 0);
            _logger.LogInformation("ResetPassword GET: token query param='{Token}' length={Length}", token ?? "NULL", token?.Length ?? 0);

            // If there is already a valid token -> redirect to the app
            if (TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorTitle = "Missing token";
                ViewBag.ErrorMessage = "Missing token. Request a new link to reset your password.";
                return View();
            }

            // Validate the token by calling the API
            try
            {
                var validationResult = await _authAPIService.ValidateResetTokenAsync(token);

                if (!validationResult.IsValid)
                {
                    if (validationResult.IsExpired)
                    {
                        ViewBag.ErrorTitle = "Expired link";
                        ViewBag.ErrorMessage = "The password reset link has expired. For security reasons, links are valid for only 30 minutes. Request a new link.";
                    }
                    else
                    {
                        ViewBag.ErrorTitle = "Invalid link";
                        ViewBag.ErrorMessage = "The password reset link is invalid. Request a new link.";
                    }
                    return View();
                }

                var model = new ResetPasswordViewModel { Token = token };
                return View(model);
            }
            catch
            {
                ViewBag.ErrorTitle = "Validation error";
                ViewBag.ErrorMessage = "An error occurred while validating the token. Please try again later.";
                return View();
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("Reset-Password"), Route("Account/Reset-Password")]
        [Route("ResetPassword"), Route("Account/ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
        {
            try
            {
                // Server-side validation based on model DataAnnotations
                if (!ModelState.IsValid)
                {
                    // Show errors in the view and keep the token in the model
                    return View(model);
                }

                var resetRequest = new
                {
                    Token = model.Token,
                    NewPassword = model.Password
                };

                var result = await _authAPIService.ResetPasswordAsync(resetRequest);

                if (result.Success)
                {
                    TempData["PasswordResetSuccessMessage"] = "Password reset successfully! You can now sign in with your new password.";
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    // Keep the model to show errors and keep the token in the form
                    ViewBag.ErrorMessage = result.Message ?? "Error during password reset.";
                    return View(model);
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again later.";
                return View(model);
            }
        }

    }
}
