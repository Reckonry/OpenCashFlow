using OpenCashFlow.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Employees;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Controllers
{
    public partial class EmployeesController : Controller
    {
        [HttpGet("New"), ActionName("New_Employee")]
        public async Task<IActionResult> NewEmployeeAsync()
        {
            PopulateLocalizationLists();
            var (sysLang, sysCountry, sysTz) = GetDefaultsFromSystem();

            // Try to load company defaults
            string lang = sysLang, country = sysCountry, tz = sysTz;
            try
            {
                var companyResp = await _companyAPIService.GetCompanyAsync();
                if (companyResp.Success && companyResp.Data != null)
                {
                    lang = companyResp.Data.DefaultLanguage ?? lang;
                    country = companyResp.Data.DefaultCountry ?? country;
                    tz = companyResp.Data.DefaultTimezone ?? tz;
                }
            }
            catch { /* fallback to system defaults */ }
            var model = new Employee_Create_DTO
            {
                UserName = string.Empty,
                UserFirstName = string.Empty,
                Email = string.Empty,
                Language = lang,
                Country = country,
                Timezone = tz
            };
            try
            {
                var rolesResp = await _roleAPIService.GetVisibleRolesAsync();
                if (rolesResp.Success && rolesResp.Data != null)
                {
                    ViewBag.Roles = rolesResp.Data; // IEnumerable<Role_List_DTO>
                }
            }
            catch { }
            return View(model);
        }

        [HttpPost("New"), ActionName("New_Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewEmployeeAsync(Employee_Create_DTO model, CancellationToken cancellationToken)
        {
            ModelState.Remove(nameof(Employee_Create_DTO.TmpFastLoginPin));
            if (!ModelState.IsValid)
            {
                // If the model is not valid, return to the view with errors
                PopulateLocalizationLists();
                // ensure defaults if empty
                var (lang, country, tz) = GetDefaultsFromSystem();
                model.Language ??= lang;
                model.Country ??= country;
                model.Timezone ??= tz;
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(model);
            }

            try
            {
                var createResult = await _employeeAPIService.CreateEmployeeAsync(model, cancellationToken);

                if (!createResult.Success)
                {
                    // Handle specific error types based on response content
                    var errorMessage = createResult.Message ?? "Error during employee creation.";

                    // Check for specific error types
                    if (errorMessage.Contains("Email already exists") || errorMessage.Contains("Email già"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "This email is already used by another employee.");
                        TempData["EmployeeErrorMessage"] = "Email already in use. An employee with this email already exists.";
                    }
                    else if (errorMessage.Contains("Formato email non valido") || errorMessage.Contains("Email is required"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "The email format is not valid.");
                        TempData["EmployeeErrorMessage"] = "Invalid email format. Enter a correct email address.";
                    }
                    else if (errorMessage.Contains("password") && (errorMessage.Contains("8 caratteri") || errorMessage.Contains("maiuscola") || errorMessage.Contains("minuscola") || errorMessage.Contains("numero") || errorMessage.Contains("speciale")))
                    {
                        ModelState.AddModelError(nameof(model.TmpNewPassword), errorMessage);
                        TempData["EmployeeErrorMessage"] = "The password does not meet security requirements. Check the listed requirements.";
                    }
                    else if (errorMessage.Contains("Password is required"))
                    {
                        ModelState.AddModelError(nameof(model.TmpNewPassword), "Password is required.");
                        TempData["EmployeeErrorMessage"] = "Enter a valid password.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, errorMessage);
                        TempData["EmployeeErrorMessage"] = errorMessage;
                    }

                    PopulateLocalizationLists();
                    try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                    return View(model);
                }

                // Positive feedback + redirect
                TempData.Remove("EmployeeErrorMessage"); // Clear any previous error messages
                TempData["EmployeeSuccessMessage"] = "Employee created successfully. The access PIN has been sent by email.";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del dipendente.");
                ModelState.AddModelError(string.Empty, "Unexpected error during creation.");
                TempData["EmployeeErrorMessage"] = "An unexpected error occurred. Please try again later.";
                PopulateLocalizationLists();
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(model);
            }
        }

    }
}
