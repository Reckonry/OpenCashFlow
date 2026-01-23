using global::Shared.Core;
using global::Shared.Models.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using global::Shared.Models;

namespace OpenCashFlow.App.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("Login"), Route("Account/Login")]
        public IActionResult Login()
        {

            var forceLogin = HttpContext.Request.Query.ContainsKey("ForceLogin");

            // If there is already a valid token -> redirect to the app
            if (!forceLogin && TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            // Show the login page
            return View();
        }


        [Route("Login"), Route("Account/Login")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Core_Credentials credential)
        {
            try
            {
                // Perform login and get the token
                ApiResponse<AuthResult> dataResponse = await _authAPIService.LoginAsync(credential.Username!, credential.Password!);

                if (dataResponse == null || dataResponse.Data == null)
                {
                    ViewBag.ErrorMessage = "Login process error!";
                    return View();
                }
                if (!dataResponse.Success)
                {
                    switch (dataResponse.Data.ErrorType)
                    {
                        case AuthErrorType.InvalidCredentials:
                            ViewBag.ErrorMessage = _localizer["Wrong username or password"];
                            return View();
                        case AuthErrorType.NotActive:
                            // Redirect to a page where the user can request a new confirmation email
                            return RedirectToAction("ResendConfirmation", new { username = credential.Username });
                        case AuthErrorType.Locked:
                            ViewBag.ErrorMessage = _localizer["Your accout is blocked"];
                            return View();
                        default:
                            ViewBag.ErrorMessage = _localizer["Unspecified error"];
                            return View();
                    }
                }
                else
                {
                    var expirationTime = TryGetJwtExpiration(dataResponse.Data.Token)
                        ?? DateTimeOffset.UtcNow.AddMinutes(Configuration.WebSessionDurationMinutes);

                    var authCookieOptions = new CookieOptions
                    {
                        Domain = _configuration["Account:CookieDomain"], // Set the cookie domain
                        HttpOnly = true, // Prevent JavaScript access to the cookie
                        Secure = true,   // Require HTTPS
                        SameSite = SameSiteMode.Lax, // SameSite policy
                        Expires = expirationTime
                    };

                    HttpContext.Response.Cookies.Append(Configuration.AuthCookieName, dataResponse.Data.Token ?? string.Empty, authCookieOptions);

                    // Info cookie without HttpOnly for JavaScript (contains only expiration)
                    var infoCookieOptions = new CookieOptions
                    {
                        Domain = _configuration["Account:CookieDomain"],
                        HttpOnly = false, // JavaScript can read it
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = expirationTime
                    };

                    HttpContext.Response.Cookies.Append(Configuration.AuthCookieName + ".Info",
                        expirationTime.ToUnixTimeSeconds().ToString(),
                        infoCookieOptions);

                    #region Fast Login cookie
                    if (dataResponse.Data.FastLoginToken == null)
                    {
                        ViewBag.ErrorMessage = "Unable to enable fast login!";
                    }
                    else
                    {                 
                        var cookieOptions = new CookieOptions
                        {
                            Domain = _configuration["Account:CookieDomain"], // Set the cookie domain
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(Configuration.FLCookieDurationMinutes)
                        };
                        Response.Cookies.Append(Configuration.FLCookieName, dataResponse.Data.FastLoginToken, cookieOptions);
                    }
                    #endregion

                    if (dataResponse.Data.RequiresPasswordChange)
                        return RedirectToAction("ChangePassword", "Home");

                    return Redirect(_configuration["Account:AppUrl"]!);
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ErrorMessage = _localizer["Unspecified error"];
                return View();
            }





        }

    }
}
