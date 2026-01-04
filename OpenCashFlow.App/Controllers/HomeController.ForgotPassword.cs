using global::Shared.Core;
using global::Shared.Models.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;

namespace OpenCashFlow.App.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("Account/ForgotPassword")]        
        public IActionResult ForgotPassword()
        {

            // Se ha già un token valido → redirect all’app
            if (TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            return View();  
        }


        [HttpPost, ValidateAntiForgeryToken]
        [Route("Account/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromForm]string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.ErrorMessage = "Inserisci un indirizzo email valido.";
                    return View();
                }

                var result = await _authAPIService.ForgotPasswordAsync(email);
                ViewBag.SuccessTitle = "Email inviata!";
                ViewBag.SuccessMessage = "Abbiamo inviato un’email con il link per reimpostare la password (controlla anche nello spam). Il link scade tra 30 minuti per motivi di sicurezza.";
                ViewBag.EmailSent = true; // Flag per nascondere il form
                return View();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Si è verificato un errore. Riprova più tardi.";
                return View();
            }
        }

    }
}
