using global::Shared.Core;
using global::Shared.Models.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;
using OpenCashFlow.App.Models;

namespace OpenCashFlow.App.Controllers
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

            // Se ha già un token valido → redirect all'app
            if (TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorTitle = "Token mancante";
                ViewBag.ErrorMessage = "Token mancante. Richiedi un nuovo link per il reset della password.";
                return View();
            }

            // Valida il token chiamando l'API
            try
            {
                var validationResult = await _authAPIService.ValidateResetTokenAsync(token);

                if (!validationResult.IsValid)
                {
                    if (validationResult.IsExpired)
                    {
                        ViewBag.ErrorTitle = "Link scaduto";
                        ViewBag.ErrorMessage = "Il link per il reset della password è scaduto. Per motivi di sicurezza, i link sono validi solo per 30 minuti. Richiedi un nuovo link.";
                    }
                    else
                    {
                        ViewBag.ErrorTitle = "Link non valido";
                        ViewBag.ErrorMessage = "Il link per il reset della password non è valido. Richiedi un nuovo link.";
                    }
                    return View();
                }

                var model = new ResetPasswordViewModel { Token = token };
                return View(model);
            }
            catch
            {
                ViewBag.ErrorTitle = "Errore di validazione";
                ViewBag.ErrorMessage = "Si è verificato un errore durante la validazione del token. Riprova più tardi.";
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
                // Validazioni lato server basate sui DataAnnotations del model
                if (!ModelState.IsValid)
                {
                    // Mostra gli errori nella view e conserva il token nel model
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
                    TempData["PasswordResetSuccessMessage"] = "Password reimpostata con successo! Ora puoi accedere con la nuova password.";
                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    // Manteniamo il model per mostrare gli errori e lasciare il token nel form
                    ViewBag.ErrorMessage = result.Message ?? "Errore durante il reset della password.";
                    return View(model);
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "Si è verificato un errore. Riprova più tardi.";
                return View(model);
            }
        }

    }
}
