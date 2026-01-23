using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Employees;
using OpenCashFlow.App.Services;

namespace OpenCashFlow.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmployeeAPIService _employeeAPIService;
        private readonly AuthenticationAPIService _authAPIService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(EmployeeAPIService employeeAPIService, AuthenticationAPIService authAPIService, ILogger<AccountController> logger, IConfiguration configuration)
        {
            _employeeAPIService = employeeAPIService;
            _authAPIService = authAPIService;
            _logger = logger;
            _configuration = configuration;
        }

        private Guid? GetCurrentUserId()
        {
            var token = HttpContext.Request.Cookies[global::Shared.Core.Configuration.AuthCookieName];
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
                return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet]
        [Route("Account")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            var detail = await _employeeAPIService.GetEmployeeByIDAsync(userId.Value);
            if (!detail.Success)
            {
                _logger.LogWarning("Impossibile ottenere i dettagli dell'utente corrente per la pagina Account: {Message}", detail.Message);
            }

            // If it fails, still pass the view (null model). The view will gracefully handle missing fields
            return View(detail.Success ? detail.Data : null);
        }

        [HttpGet]
        [Route("Account/Edit"), ActionName("Edit_Account")]
        public async Task<IActionResult> EditAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            var detail = await _employeeAPIService.GetEmployeeByIDAsync(userId.Value);
            if (!detail.Success || detail.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load profile data";
                return View("Edit_Account", new Employee_MyProfile_Update_DTO());
            }

            var vm = new Employee_MyProfile_Update_DTO
            {
                UserFirstName = detail.Data.UserFirstName,
                UserLastName = detail.Data.UserLastName,
                Email = detail.Data.Email,
                PhoneNumberPrefix = detail.Data.PhoneNumberPrefix,
                PhoneNumber = detail.Data.PhoneNumber,
                Language = detail.Data.Language,
                Country = detail.Data.Country,
                Timezone = detail.Data.Timezone
            };

            return View("Edit_Account", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/Edit"), ActionName("Edit_Account")]
        public async Task<IActionResult> EditAccount(Employee_MyProfile_Update_DTO model)
        {
            if (!ModelState.IsValid)
                return View("Edit_Account", model);

            var result = await _employeeAPIService.UpdateMyProfileAsync(model);
            if (!result.Success)
            {
                ViewBag.ErrorMessage = result.Message ?? "Error during save";
                return View("Edit_Account", model);
            }

            // Regenerate the JWT token with updated claims to update the navbar without logout
            var tokenResult = await _authAPIService.RegenerateTokenAsync();
            if (!tokenResult.Success)
            {
                _logger.LogWarning("Token regeneration failed after profile update: {Message}", tokenResult.Message);
            }
            else if (tokenResult.Data != null && !string.IsNullOrWhiteSpace(tokenResult.Data.Token))
            {
                var newToken = tokenResult.Data.Token;
                var expirationTime = TryGetJwtExpiration(newToken)
                    ?? DateTimeOffset.UtcNow.AddMinutes(global::Shared.Core.Configuration.WebSessionDurationMinutes);

                var authCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                HttpContext.Response.Cookies.Append(global::Shared.Core.Configuration.AuthCookieName, newToken, authCookieOptions);

                var infoCookieOptions = new CookieOptions
                {
                    Domain = _configuration["Account:CookieDomain"],
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expirationTime
                };

                HttpContext.Response.Cookies.Append(global::Shared.Core.Configuration.AuthCookieName + ".Info",
                    expirationTime.ToUnixTimeSeconds().ToString(),
                    infoCookieOptions);
            }

            ViewBag.SuccessMessage = "Profile updated";

            // Reload updated data for the view
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                var detail = await _employeeAPIService.GetEmployeeByIDAsync(userId.Value);
                if (detail.Success && detail.Data != null)
                {
                    var updatedVm = new Employee_MyProfile_Update_DTO
                    {
                        UserFirstName = detail.Data.UserFirstName,
                        UserLastName = detail.Data.UserLastName,
                        Email = detail.Data.Email,
                        PhoneNumberPrefix = detail.Data.PhoneNumberPrefix,
                        PhoneNumber = detail.Data.PhoneNumber,
                        Language = detail.Data.Language,
                        Country = detail.Data.Country,
                        Timezone = detail.Data.Timezone
                    };
                    return View("Edit_Account", updatedVm);
                }
            }

            return View("Edit_Account", model);
        }

        public class PasswordVM
        {
            public string? NewPassword { get; set; }
            public string? RepeatPassword { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/ChangePassword")]
        public async Task<IActionResult> ChangePassword(PasswordVM vm)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Home");

            if (string.IsNullOrWhiteSpace(vm.NewPassword) || vm.NewPassword != vm.RepeatPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match";
                return await LoadEditAccountView(userId.Value);
            }

            var resp = await _authAPIService.ChangePasswordRequiredAsync(vm.NewPassword);
            if (!resp.Success)
            {
                ViewBag.ErrorMessage = resp.Message ?? "Unable to change password";
                return await LoadEditAccountView(userId.Value);
            }

            ViewBag.SuccessMessage = "Password updated";
            return await LoadEditAccountView(userId.Value);
        }

        private async Task<IActionResult> LoadEditAccountView(Guid userId)
        {
            var detail = await _employeeAPIService.GetEmployeeByIDAsync(userId);
            if (!detail.Success || detail.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load profile data";
                return View("Edit_Account", new Employee_MyProfile_Update_DTO());
            }

            var vm = new Employee_MyProfile_Update_DTO
            {
                UserFirstName = detail.Data.UserFirstName,
                UserLastName = detail.Data.UserLastName,
                Email = detail.Data.Email,
                PhoneNumberPrefix = detail.Data.PhoneNumberPrefix,
                PhoneNumber = detail.Data.PhoneNumber,
                Language = detail.Data.Language,
                Country = detail.Data.Country,
                Timezone = detail.Data.Timezone
            };

            return View("Edit_Account", vm);
        }

        private DateTimeOffset? TryGetJwtExpiration(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var expClaim = jwt.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, JwtRegisteredClaimNames.Exp, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Type, "exp", StringComparison.OrdinalIgnoreCase))?.Value;

                if (long.TryParse(expClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(seconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossibile leggere l'exp dal token JWT rigenerato.");
            }

            return null;
        }
    }
}
