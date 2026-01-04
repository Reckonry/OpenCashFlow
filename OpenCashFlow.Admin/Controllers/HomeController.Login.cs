using global::Shared.Core;
using global::Shared.Models.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using global::Shared.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace OpenCashFlow.Admin.Controllers
{
    public partial class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("Login"), HttpGet("Account/Login")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Login()
        {

            var forceLogin = HttpContext.Request.Query.ContainsKey("ForceLogin");

            // Se ha già un token valido → redirect all’app
            if (!forceLogin && TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:ManagementUrl"]!);
            // Mostra la pagina di login
            return View("Login");
        }

        [HttpPost("Login"), HttpPost("Account/Login")]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Login([Bind] Core_Credentials credential)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = _localizer["LoginGenericError"];
                return View("Login");
            }
            try
            {
                // Esegui il login e ottieni il token
                ApiResponse<AuthResult> dataResponse = await _authAPIService.LoginAsync(credential.Username!, credential.Password!);

                if (dataResponse == null || dataResponse.Data == null)
                {
                    ViewBag.ErrorMessage = _localizer["LoginProcessError"];
                    return View("Login");
                }
                if (!dataResponse.Success)
                {
                    switch (dataResponse.Data.ErrorType)
                    {
                        case AuthErrorType.InvalidCredentials:
                            ViewBag.ErrorMessage = _localizer["LoginInvalidCredentials"];
                            return View("Login");
                        case AuthErrorType.NotActive:
                            // Redirect to a page where the user can request a new confirmation email
                            return RedirectToAction("ResendConfirmation", new { username = credential.Username });
                        case AuthErrorType.Locked:
                            ViewBag.ErrorMessage = _localizer["LoginAccountLocked"];
                            return View("Login");
                        default:
                            ViewBag.ErrorMessage = _localizer["LoginGenericError"];
                            return View("Login");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dataResponse.Data.Token))
                    {
                        ViewBag.ErrorMessage = _localizer["LoginGenericError"];
                        return View("Login");
                    }

                    var authCookieOptions = new CookieOptions
                    {
                        Domain = _configuration["Account:CookieDomain"], // Imposta il dominio del cookie
                        HttpOnly = true, // Impedisce l'accesso al cookie via JavaScript
                        Secure = true,   // Richiede HTTPS
                        SameSite = SameSiteMode.Lax, // Politica SameSite
                        Path = "/"
                    };
                    if (credential.RememberMe)
                    {
                        var rememberLifetime = TimeSpan.FromMinutes(Configuration.WebSessionRememberDurationMinutes);
                        authCookieOptions.Expires = DateTimeOffset.UtcNow.Add(rememberLifetime);
                        authCookieOptions.MaxAge = rememberLifetime;
                    }
                    HttpContext.Response.Cookies.Append(Configuration.AuthCookieName, dataResponse.Data.Token, authCookieOptions);

                    if (dataResponse.Data.RequiresPasswordChange)
                        return RedirectToAction("ChangePassword", "Home");

                    return Redirect(_configuration["Account:ManagementUrl"]!);
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ErrorMessage = _localizer["LoginGenericError"];
                return View("Login");
            }
            catch (TaskCanceledException)
            {
                ViewBag.ErrorMessage = _localizer["LoginGenericError"];
                return View("Login");
            }

        }

    }
}
