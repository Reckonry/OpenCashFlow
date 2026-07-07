using OpenCashFlow.Contracts.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class HomeController : Controller
    {
        [HttpGet]
        [Route("FastLogin"), Route("Account/FastLogIn")]
        public IActionResult FastLogin()
        {

            // If there is already a valid token -> redirect to the app
            if (TryValidateAuthCookie(out _))
                return Redirect(_configuration["Account:AppUrl"]!);

            // If there is no FLCookie, go to basic login
            if (!Request.Cookies.ContainsKey(Configuration.FLCookieName))
                return Redirect(_configuration["Account:Login"]!);

            // Check for temporary lockout and show a message
            const string lockoutCookieName = "FL_LockoutUntil";
            if (Request.Cookies.TryGetValue(lockoutCookieName, out var lockoutRaw)
                && long.TryParse(lockoutRaw, out var untilUnix))
            {
                var now = DateTimeOffset.UtcNow;
                var until = DateTimeOffset.FromUnixTimeSeconds(untilUnix);
                if (until > now)
                {
                    var remaining = (int)Math.Ceiling((until - now).TotalSeconds);
                    ViewBag.ErrorMessage = _localizer[$"Too many attempts. Try again in {remaining} seconds."];
                }
            }

            return View();
        }


        [Route("FastLogin"), Route("Account/FastLogIn")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FastLogIn([Bind] string Pin)
        {
            // Lockout parameters
            const int maxAttempts = 5;           // allowed attempts
            const int lockoutSeconds = 30;       // temporary lockout
            const string attemptsCookieName = "FL_Attempts";
            const string lockoutCookieName = "FL_LockoutUntil";

            var now = DateTimeOffset.UtcNow;

            try
            {
                // If in lockout, block immediately
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



                // Save the token (e.g., in cookies or local storage)
                var expirationTime = TryGetJwtExpiration(token)
                    ?? DateTimeOffset.UtcNow.AddMinutes(Configuration.WebSessionDurationMinutes);

                var authCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"], // Set the cookie domain
                    HttpOnly = true, // Prevent JavaScript access to the cookie
                    Secure = true,   // Require HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                HttpContext.Response.Cookies.Append(Configuration.AuthCookieName, token, authCookieOptions);

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

                // Clear attempts/lockout cookies after success
                HttpContext.Response.Cookies.Delete(attemptsCookieName, new CookieOptions { Domain = _configuration["Account:CookieDomain"], Secure = true, SameSite = SameSiteMode.Lax });
                HttpContext.Response.Cookies.Delete(lockoutCookieName, new CookieOptions { Domain = _configuration["Account:CookieDomain"], Secure = true, SameSite = SameSiteMode.Lax });

                // 2. Retrieve features from the API using the token
                // var features = await _featureClient.GetFeaturesAsync(token);

                // 3. Save features in the session
                //HttpContext.Session.SetString("features", JsonSerializer.Serialize(features));
                //await HttpContext.Session.CommitAsync();

                return Redirect(_configuration["Account:AppUrl"]!);
                //return Redirect("~/");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // TODO: use a proper alert message
                ViewBag.ErrorMessage = _localizer["Wrong PIN!"];

                
                    // Increment failed attempts
                    int attempts = 0;
                    if (Request.Cookies.TryGetValue(attemptsCookieName, out var attemptsRaw))
                        int.TryParse(attemptsRaw, out attempts);

                    attempts++;

                    if (attempts >= maxAttempts)
                    {
                        // Set lockout and reset counter
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
                        // Update attempts counter with a short expiration
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
