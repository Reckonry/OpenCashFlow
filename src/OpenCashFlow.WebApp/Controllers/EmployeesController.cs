using OpenCashFlow.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.DTOs.Employees;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Controllers
{
    [Authorize(Roles = "CompanyAdmin")]
    [Route("[Controller]")]
    public partial class EmployeesController(EmployeeAPIService EmployeeAPIService, CompanyAPIService CompanyAPIService, ILogger<EmployeesController> Logger, RoleAPIService RoleAPIService) : Controller
    {
        private readonly EmployeeAPIService _employeeAPIService = EmployeeAPIService;
        private readonly CompanyAPIService _companyAPIService = CompanyAPIService;
        private readonly ILogger<EmployeesController> _logger = Logger;
        private readonly RoleAPIService _roleAPIService = RoleAPIService;

        public async Task<IActionResult> IndexAsync()
        {
            ApiResponse<IEnumerable<Employee_List_DTO>?> x = await _employeeAPIService.GetEmployeesAsync();
            ApiResponse<Company_Detail_DTO> company = await _companyAPIService.GetCompanyAsync();
            ViewBag.MaxEmployees = (company.Data?.MaxUsers ?? 15L);
            return View(x.Data);
        }

        [HttpGet("View/{UserID}"), ActionName("View_Employee")]
        public async Task<IActionResult> ViewEmployeeAsync(Guid UserID)
        {
            ApiResponse<Employee_Detail_DTO?> x = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
            return View(x.Data);
        }

        [HttpPost("Delete/{UserID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(Guid UserID)
        {
            _logger.LogInformation("DeleteEmployee called for UserID: {UserID}", UserID);

            try
            {
                var result = await _employeeAPIService.DeleteEmployeeAsync(UserID);

                _logger.LogInformation("DeleteEmployee API result for UserID {UserID}: Success={Success}, Message='{Message}'",
                    UserID, result.Success, result.Message);

                if (result.Success)
                {
                    TempData["EmployeeSuccessMessage"] = "Employee deleted successfully.";
                    _logger.LogInformation("Employee deletion successful for UserID: {UserID}", UserID);
                }
                else
                {
                    var errorMessage = result.Message ?? "Error during employee deletion.";
                    TempData["EmployeeErrorMessage"] = errorMessage;
                    _logger.LogWarning("Employee deletion failed for UserID {UserID}: {ErrorMessage}", UserID, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del dipendente {UserID}", UserID);
                TempData["EmployeeErrorMessage"] = "An unexpected error occurred during deletion.";
            }

            return RedirectToAction("Index");
        }

        #region Helpers
        private void PopulateLocalizationLists()
        {
            // Timezones: use system time zones (IANA on Linux/macOS, Windows IDs on Windows)
            var tzItems = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new
                {
                    Id = tz.Id,
                    Display = $"(UTC{(tz.BaseUtcOffset >= TimeSpan.Zero ? "+" : "-")}{tz.BaseUtcOffset:hh\\:mm}) {tz.DisplayName}"
                })
                .Distinct()
                .OrderBy(t => t.Display)
                .ToList();
            ViewBag.Timezones = tzItems;

            // Languages: distinct two-letter codes from specific cultures
            var languageCodes = System.Globalization.CultureInfo
                .GetCultures(System.Globalization.CultureTypes.SpecificCultures)
                .Select(c => c.TwoLetterISOLanguageName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            var languages = languageCodes
                .Select(code => new
                {
                    Code = code,
                    Name = new System.Globalization.CultureInfo(code).NativeName
                })
                .OrderBy(l => l.Name)
                .ToList();
            ViewBag.Languages = languages;

            // Countries: unique by ISO 2-letter region code from specific cultures
            var countries = System.Globalization.CultureInfo
                .GetCultures(System.Globalization.CultureTypes.SpecificCultures)
                .Select(c => new System.Globalization.RegionInfo(c.Name))
                .GroupBy(r => r.TwoLetterISORegionName)
                .Select(g => g.First())
                .Select(r => new { Code = r.TwoLetterISORegionName, Name = r.EnglishName })
                .OrderBy(r => r.Name)
                .ToList();
            ViewBag.Countries = countries;
        }

        private static (string Lang, string Country, string Timezone) GetDefaultsFromSystem()
        {
            // Base culture settings
            var uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            var lang = uiCulture.TwoLetterISOLanguageName;

            string country;
            try
            {
                // RegionInfo requires a specific culture (e.g., "en-US") or a region code (e.g., "US").
                // If the current culture is neutral (e.g., "en"), create a specific culture from it.
                var specificCulture = culture.IsNeutralCulture
                    ? System.Globalization.CultureInfo.CreateSpecificCulture(culture.Name)
                    : culture;

                var region = new System.Globalization.RegionInfo(specificCulture.Name);
                country = region.TwoLetterISORegionName;
            }
            catch
            {
                country = "IT"; // sensible default
            }

            var tz = TimeZoneInfo.Local.Id;

            return (lang, country, tz);
        }
        #endregion
    }
}
