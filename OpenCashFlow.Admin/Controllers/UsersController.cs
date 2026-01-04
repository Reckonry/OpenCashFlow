using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenCashFlow.Admin.Services;
using global::Shared.DTOs.Admin;
using System.Text.Json;

namespace OpenCashFlow.Admin.Controllers
{
    [Authorize(Policy = "GIManagers")]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UserManagementAPIService _userManagementService;
        private readonly CompanyAPIService _companyAPIService;

        public UsersController(
            ILogger<UsersController> logger,
            UserManagementAPIService userManagementService,
            CompanyAPIService companyAPIService)
        {
            _logger = logger;
            _userManagementService = userManagementService;
            _companyAPIService = companyAPIService;
        }

        /// <summary>
        /// GET /Admin/Users - Lista utenti con filtri
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] User_Filter_DTO? filters)
        {
            filters ??= new User_Filter_DTO { Page = 1, PageSize = 50 };

            var result = await _userManagementService.GetUsersAsync(filters, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new { users = new List<User_List_DTO>(), totalCount = 0, filters });
            }

            // Parse the dynamic response
            var jsonElement = (JsonElement)result.Data!;
            var usersJson = jsonElement.GetProperty("users").GetRawText();
            var users = JsonSerializer.Deserialize<List<User_List_DTO>>(usersJson) ?? new List<User_List_DTO>();
            var totalCount = jsonElement.GetProperty("totalCount").GetInt32();

            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = jsonElement.GetProperty("totalPages").GetInt32();
            ViewBag.CurrentPage = filters.Page;
            ViewBag.Filters = filters;

            return View(users);
        }

        /// <summary>
        /// GET /Admin/Users/Details/{id} - Dettaglio utente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _userManagementService.GetUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Utente non trovato";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        /// <summary>
        /// GET /Admin/Users/Create - Form creazione utente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new User_Create_DTO());
        }

        /// <summary>
        /// POST /Admin/Users/Create - Creazione utente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User_Create_DTO dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewData();
                return View(dto);
            }

            var result = await _userManagementService.CreateUserAsync(dto, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nella creazione dell'utente";
                await LoadViewData();
                return View(dto);
            }

            TempData["Success"] = "Utente creato con successo";
            return RedirectToAction(nameof(Details), new { id = result.Data?.UserID });
        }

        /// <summary>
        /// GET /Admin/Users/Edit/{id} - Form modifica utente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _userManagementService.GetUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Utente non trovato";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData();

            var dto = new User_Update_DTO
            {
                UserID = result.Data.UserID,
                Email = result.Data.Email,
                FirstName = result.Data.FirstName,
                LastName = result.Data.LastName,
                PhoneNumber = result.Data.PhoneNumber,
                IsActive = result.Data.IsActive,
                TenantID = result.Data.TenantID
            };

            return View(dto);
        }

        /// <summary>
        /// POST /Admin/Users/Edit/{id} - Aggiornamento utente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User_Update_DTO dto)
        {
            if (id != dto.UserID)
            {
                TempData["Error"] = "ID utente non corrispondente";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadViewData();
                return View(dto);
            }

            var result = await _userManagementService.UpdateUserAsync(id, dto, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nell'aggiornamento dell'utente";
                await LoadViewData();
                return View(dto);
            }

            TempData["Success"] = "Utente aggiornato con successo";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Lock/{id} - Blocca utente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(Guid id, DateTime? lockoutEnd = null)
        {
            var result = await _userManagementService.LockUserAsync(id, lockoutEnd, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nel blocco dell'utente";
            }
            else
            {
                TempData["Success"] = "Utente bloccato con successo";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Unlock/{id} - Sblocca utente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(Guid id)
        {
            var result = await _userManagementService.UnlockUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nello sblocco dell'utente";
            }
            else
            {
                TempData["Success"] = "Utente sbloccato con successo";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/ResetPassword/{id} - Reset password
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id, string newPassword, bool requirePasswordChange = true)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Password non valida";
                return RedirectToAction(nameof(Details), new { id });
            }

            var dto = new User_ResetPassword_DTO
            {
                UserID = id,
                NewPassword = newPassword,
                RequirePasswordChange = requirePasswordChange,
                SendNotificationEmail = true
            };

            var result = await _userManagementService.ResetPasswordAsync(id, dto, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nel reset della password";
            }
            else
            {
                TempData["Success"] = "Password reimpostata con successo";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/UpdateRoles/{id} - Aggiorna ruoli
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(Guid id, string[] roles)
        {
            var dto = new User_Roles_DTO
            {
                UserID = id,
                Roles = roles ?? Array.Empty<string>()
            };

            var result = await _userManagementService.UpdateUserRolesAsync(id, dto, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nell'aggiornamento dei ruoli";
            }
            else
            {
                TempData["Success"] = "Ruoli aggiornati con successo";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Delete/{id} - Elimina utente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userManagementService.DeleteUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Errore nell'eliminazione dell'utente";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = "Utente eliminato con successo";
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task LoadViewData()
        {
            // Load companies
            var companiesResponse = await _companyAPIService.GetCompaniesAsync();
            ViewBag.Companies = companiesResponse.Data ?? new List<global::Shared.DTOs.Company_Detail_DTO>();

            // Load roles
            var rolesResponse = await _userManagementService.GetRolesAsync(HttpContext.RequestAborted);
            ViewBag.Roles = rolesResponse.Data ?? new List<Role_DTO>();
        }

        #endregion
    }
}
