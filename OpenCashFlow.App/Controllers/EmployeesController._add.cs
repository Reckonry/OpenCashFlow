using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Employees;
using global::Shared.Models;

namespace OpenCashFlow.App.Controllers
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
                // Se il modello non è valido, ritorna alla view con gli errori
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
                    var errorMessage = createResult.Message ?? "Errore durante la creazione del dipendente.";

                    // Check for specific error types
                    if (errorMessage.Contains("Email already exists") || errorMessage.Contains("Email già"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Questa email è già utilizzata da un altro dipendente.");
                        TempData["EmployeeErrorMessage"] = "Email già in uso. Un dipendente con questa email esiste già.";
                    }
                    else if (errorMessage.Contains("Formato email non valido") || errorMessage.Contains("Email is required"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Il formato dell'email non è valido.");
                        TempData["EmployeeErrorMessage"] = "Formato email non valido. Inserisci un indirizzo email corretto.";
                    }
                    else if (errorMessage.Contains("password") && (errorMessage.Contains("8 caratteri") || errorMessage.Contains("maiuscola") || errorMessage.Contains("minuscola") || errorMessage.Contains("numero") || errorMessage.Contains("speciale")))
                    {
                        ModelState.AddModelError(nameof(model.TmpNewPassword), errorMessage);
                        TempData["EmployeeErrorMessage"] = "La password non rispetta i requisiti di sicurezza. Controlla i requisiti indicati.";
                    }
                    else if (errorMessage.Contains("Password is required"))
                    {
                        ModelState.AddModelError(nameof(model.TmpNewPassword), "La password è obbligatoria.");
                        TempData["EmployeeErrorMessage"] = "Inserisci una password valida.";
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

                // Feedback positivo + redirect
                TempData.Remove("EmployeeErrorMessage"); // Pulisce eventuali messaggi di errore precedenti
                TempData["EmployeeSuccessMessage"] = "Dipendente creato con successo. Il PIN di accesso è stato inviato via email.";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del dipendente.");
                ModelState.AddModelError(string.Empty, "Errore inatteso durante la creazione.");
                TempData["EmployeeErrorMessage"] = "Si è verificato un errore imprevisto. Riprova più tardi.";
                PopulateLocalizationLists();
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(model);
            }
        }

    }
}
