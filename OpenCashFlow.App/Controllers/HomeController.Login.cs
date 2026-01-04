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

            // Se ha già un token valido → redirect all’app
            if (!forceLogin && TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            // Mostra la pagina di login
            return View();
        }


        [Route("Login"), Route("Account/Login")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Core_Credentials credential)
        {
            try
            {
                // Esegui il login e ottieni il token
                ApiResponse<AuthResult> dataResponse = await _authAPIService.LoginAsync(credential.Username!, credential.Password!);

                if (dataResponse == null || dataResponse.Data == null)
                {
                    ViewBag.ErrorMessage = "Errore nel processo di login!";
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
                        Domain = _configuration["Account:CookieDomain"], // Imposta il dominio del cookie
                        HttpOnly = true, // Impedisce l'accesso al cookie via JavaScript
                        Secure = true,   // Richiede HTTPS
                        SameSite = SameSiteMode.Lax, // Politica SameSite
                        Expires = expirationTime
                    };

                    HttpContext.Response.Cookies.Append(Configuration.AuthCookieName, dataResponse.Data.Token ?? string.Empty, authCookieOptions);

                    // Cookie info senza HttpOnly per JavaScript (contiene solo la scadenza)
                    var infoCookieOptions = new CookieOptions
                    {
                        Domain = _configuration["Account:CookieDomain"],
                        HttpOnly = false, // JavaScript può leggerlo
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
                        ViewBag.ErrorMessage = "Impossibile abilitare fast login!";
                    }
                    else
                    {                 
                        var cookieOptions = new CookieOptions
                        {
                            Domain = _configuration["Account:CookieDomain"], // Imposta il dominio del cookie
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
