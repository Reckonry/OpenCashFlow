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

            // If there is already a valid token -> redirect to the app
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
                    ViewBag.ErrorMessage = "Please enter a valid email address.";
                    return View();
                }

                var result = await _authAPIService.ForgotPasswordAsync(email);
                ViewBag.SuccessTitle = "Email sent!";
                ViewBag.SuccessMessage = "We sent an email with the link to reset your password (check your spam folder too). The link expires in 30 minutes for security reasons.";
                ViewBag.EmailSent = true; // Flag to hide the form
                return View();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again later.";
                return View();
            }
        }

    }
}
