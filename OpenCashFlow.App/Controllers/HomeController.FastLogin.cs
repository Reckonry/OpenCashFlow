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
        [Route("FastLogin"), Route("Account/FastLogIn")]
        public IActionResult FastLogin()
        {

            // Se ha già un token valido → redirect all’app
            if (TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            // Se non c’è FLCookie, passo a login di base
            if (!Request.Cookies.ContainsKey(Configuration.FLCookieName))
                return Redirect(_configuration["Account:Login"]!);

            // Controllo eventuale lockout temporaneo e mostro messaggio
            const string lockoutCookieName = "FL_LockoutUntil";
            if (Request.Cookies.TryGetValue(lockoutCookieName, out var lockoutRaw)
                && long.TryParse(lockoutRaw, out var untilUnix))
            {
                var now = DateTimeOffset.UtcNow;
                var until = DateTimeOffset.FromUnixTimeSeconds(untilUnix);
                if (until > now)
                {
                    var remaining = (int)Math.Ceiling((until - now).TotalSeconds);
                    ViewBag.ErrorMessage = _localizer[$"Troppi tentativi. Riprova tra {remaining} secondi."];
                }
            }

            return View();
        }


        [Route("FastLogin"), Route("Account/FastLogIn")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FastLogIn([Bind] string Pin)
        {
            // Parametri lockout
            const int maxAttempts = 5;           // tentativi consentiti
            const int lockoutSeconds = 30;       // blocco temporaneo
            const string attemptsCookieName = "FL_Attempts";
            const string lockoutCookieName = "FL_LockoutUntil";

            var now = DateTimeOffset.UtcNow;

            try
            {
                // Se in lockout, blocca subito
                if (Request.Cookies.TryGetValue(lockoutCookieName, out var lockoutRaw)
                    && long.TryParse(lockoutRaw, out var untilUnix))
                {
                    var until = DateTimeOffset.FromUnixTimeSeconds(untilUnix);
                    if (until > now)
                    {
                        var remaining = (int)Math.Ceiling((until - now).TotalSeconds);
                        ViewBag.ErrorMessage = _localizer[$"Too many attempts. try again in {remaining} seconds."];
                        return View();
                    }
                }
                if (!Request.Cookies.TryGetValue(Configuration.FLCookieName, out var protectedValue))
                {
                    ModelState.AddModelError("", _localizer["Fast Login not available."]);
                    return View();
                }

                var token = await _authAPIService.FastLoginAsync(Pin, protectedValue);



                // Salva il token (ad esempio nei cookie o local storage)
                var expirationTime = TryGetJwtExpiration(token)
                    ?? DateTimeOffset.UtcNow.AddMinutes(Configuration.WebSessionDurationMinutes);

                var authCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"], // Imposta il dominio del cookie
                    HttpOnly = true, // Impedisce l'accesso al cookie via JavaScript
                    Secure = true,   // Richiede HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                HttpContext.Response.Cookies.Append(Configuration.AuthCookieName, token, authCookieOptions);

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

                // Pulisce cookie tentativi/lockout dopo successo
                HttpContext.Response.Cookies.Delete(attemptsCookieName, new CookieOptions { Domain = _configuration["Account:CookieDomain"], Secure = true, SameSite = SameSiteMode.Lax });
                HttpContext.Response.Cookies.Delete(lockoutCookieName, new CookieOptions { Domain = _configuration["Account:CookieDomain"], Secure = true, SameSite = SameSiteMode.Lax });

                // 2. Recupera le feature dall’API usando il token
                // var features = await _featureClient.GetFeaturesAsync(token);

                // 3. Salva le feature nella sessione
                //HttpContext.Session.SetString("features", JsonSerializer.Serialize(features));
                //await HttpContext.Session.CommitAsync();

                return Redirect(_configuration["Account:AppUrl"]!);
                //return Redirect("~/");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                //todo: usare proper allert message
                ViewBag.ErrorMessage = _localizer["Wrong PIN!"];

                
                    // Incrementa tentativi falliti
                    int attempts = 0;
                    if (Request.Cookies.TryGetValue(attemptsCookieName, out var attemptsRaw))
                        int.TryParse(attemptsRaw, out attempts);

                    attempts++;

                    if (attempts >= maxAttempts)
                    {
                        // Imposta lockout e azzera contatore
                        var until = now.AddSeconds(lockoutSeconds);
                        HttpContext.Response.Cookies.Append(lockoutCookieName, until.ToUnixTimeSeconds().ToString(), new CookieOptions
                        {
                            Domain = _configuration["Account:CookieDomain"],
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = until
                        });

                        // Reset attempts
                        HttpContext.Response.Cookies.Append(attemptsCookieName, "0", new CookieOptions
                        {
                            Domain = _configuration["Account:CookieDomain"],
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = until
                        });

                        ViewBag.ErrorMessage = _localizer[$"Too many attempts. Try again in {lockoutSeconds} seconds."];
                    }
                    else
                    {
                        // Aggiorna contatore tentativi con scadenza breve
                        HttpContext.Response.Cookies.Append(attemptsCookieName, attempts.ToString(), new CookieOptions
                        {
                            Domain = _configuration["Account:CookieDomain"],
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = now.AddMinutes(10)
                        });

                        var remaining = maxAttempts - attempts;
                        ViewBag.ErrorMessage = remaining > 0
                            ? _localizer[$"Wrong PIN. Remaining attempts: {remaining}."]
                            : _localizer["Wrong PIN."];
                    }

                    //_logger.LogError("Login failed for user {Username}", credential.Username);
               

                return View();
            }
        }

    }
}
