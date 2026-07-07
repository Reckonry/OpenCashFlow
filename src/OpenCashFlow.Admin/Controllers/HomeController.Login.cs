using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Infrastructure.Persistence.Entities.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenCashFlow.Infrastructure.Persistence.Entities;
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

            // If there is already a valid token, redirect to the app
            if (!forceLogin && TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:ManagementUrl"]!);
            // Show the login page
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
                // Perform login and get the token
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
                        Domain = _configuration["Account:CookieDomain"], // Set the cookie domain
                        HttpOnly = true, // Prevent JavaScript access to the cookie
                        Secure = true,   // Require HTTPS
                        SameSite = SameSiteMode.Lax, // SameSite policy
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
