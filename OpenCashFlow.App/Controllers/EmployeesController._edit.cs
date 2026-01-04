using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Employees;
using global::Shared.Models;

namespace OpenCashFlow.App.Controllers
{
    public partial class EmployeesController : Controller
    {
        [HttpGet("edit/{UserID}"), ActionName("Edit_Employee")]
        public async Task<IActionResult> EditEmployeeAsync(Guid UserID)
        {
            ApiResponse<Employee_Detail_DTO?> x = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
            PopulateLocalizationLists();
            // Load visible roles for selection
            try
            {
                var rolesResp = await _roleAPIService.GetVisibleRolesAsync();
                if (rolesResp.Success && rolesResp.Data != null)
                {
                    ViewBag.Roles = rolesResp.Data; // IEnumerable<Role_List_DTO>
                }
            }
            catch { /* ignore, roles list will be empty */ }

            // If some localization fields are missing on the user, prefill using company defaults
            try
            {
                var companyResp = await _companyAPIService.GetCompanyAsync();
                if (x.Data != null && companyResp.Success && companyResp.Data != null)
                {
                    x.Data.Language = string.IsNullOrWhiteSpace(x.Data.Language)
                        ? (companyResp.Data.DefaultLanguage ?? x.Data.Language)
                        : x.Data.Language;
                    x.Data.Country = string.IsNullOrWhiteSpace(x.Data.Country)
                        ? (companyResp.Data.DefaultCountry ?? x.Data.Country)
                        : x.Data.Country;
                    x.Data.Timezone = string.IsNullOrWhiteSpace(x.Data.Timezone)
                        ? (companyResp.Data.DefaultTimezone ?? x.Data.Timezone)
                        : x.Data.Timezone;
                }
            }
            catch { /* ignore */ }

            return View(x.Data);
        }

        [HttpPost("Edit/{UserID}"), ActionName("Edit_Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployeeAsync(Guid UserID, Employee_Update_DTO model, CancellationToken cancellationToken)
        {
            if (UserID != model.UserID)
            {
                ModelState.AddModelError(string.Empty, "ID del dipendente non corrisponde.");
                var fallback = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
                PopulateLocalizationLists();
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(fallback.Data);
            }

            if (!ModelState.IsValid)
            {
                var fallback = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
                PopulateLocalizationLists();
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(fallback.Data);
            }

            try
            {
                var updateResult = await _employeeAPIService.UpdateEmployeeAsync(UserID, model);

                if (!updateResult.Success)
                {
                    // Handle specific error types based on response content
                    var errorMessage = updateResult.Message ?? "Errore durante l'aggiornamento del dipendente.";

                    // Check for specific error types
                    if (errorMessage.Contains("Email già utilizzata"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Questa email è già utilizzata da un altro dipendente.");
                        TempData["EmployeeErrorMessage"] = "Email già in uso. Verifica l'indirizzo email inserito.";
                    }
                    else if (errorMessage.Contains("Formato email non valido"))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Il formato dell'email non è valido.");
                        TempData["EmployeeErrorMessage"] = "Formato email non valido. Inserisci un indirizzo email corretto.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, errorMessage);
                        TempData["EmployeeErrorMessage"] = errorMessage;
                    }

                    var fallback = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
                    PopulateLocalizationLists();
                    try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                    return View(fallback.Data);
                }

                // Use detailed response to create specific message
                var responseData = updateResult.Data;
                if (responseData != null)
                {
                    TempData["EmployeeSuccessMessage"] = responseData.Message;
                }
                else
                {
                    TempData["EmployeeSuccessMessage"] = "Dipendente aggiornato con successo.";
                }

                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la modifica del dipendente.");
                ModelState.AddModelError(string.Empty, "Errore imprevisto durante la modifica.");
                TempData["EmployeeErrorMessage"] = "Si è verificato un errore imprevisto. Riprova più tardi.";
                var fallback = await _employeeAPIService.GetEmployeeByIDAsync(UserID);
                PopulateLocalizationLists();
                try { var rolesResp = await _roleAPIService.GetVisibleRolesAsync(); if (rolesResp.Success && rolesResp.Data != null) ViewBag.Roles = rolesResp.Data; } catch {}
                return View(fallback.Data);
            }
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost("Edit/SendPasswordReset"), ActionName("SendPasswordReset")]
        public async Task<IActionResult> SendPasswordReset(Guid userId)
        {
            await _employeeAPIService.SendPasswordResetAsync(userId);

            // reindirizzare o restituire un risultato appropriato
            return RedirectToAction("Details", "Employee", new { id = userId });
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Edit/ResendPin/{UserID}"), ActionName("ResendPin")]
        public async Task<IActionResult> ResendPin(Guid UserID)
        {
            try
            {
                var result = await _employeeAPIService.ResendPinAsync(UserID);

                if (result.Success)
                {
                    TempData["EmployeeSuccessMessage"] = "PIN inviato con successo all'indirizzo email del dipendente.";
                }
                else
                {
                    // Analizza il messaggio di errore per fornire feedback più specifico
                    var errorMessage = result.Message ?? "";
                    if (errorMessage.Contains("Rate limit"))
                    {
                        TempData["EmployeeErrorMessage"] = "Troppi invii email. Riprova tra qualche minuto.";
                    }
                    else if (errorMessage.Contains("policy") || errorMessage.Contains("violation"))
                    {
                        TempData["EmployeeErrorMessage"] = "Indirizzo email non valido o bloccato dal provider. Verifica l'indirizzo email del dipendente.";
                    }
                    else
                    {
                        TempData["EmployeeErrorMessage"] = "Errore durante l'invio del PIN. Verifica che l'email sia configurata correttamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il re-invio del PIN per l'utente {UserID}", UserID);
                TempData["EmployeeErrorMessage"] = "Si è verificato un errore durante l'invio del PIN. Riprova più tardi.";
            }

            return RedirectToAction("Edit_Employee", new { UserID });
        }
    }
}
