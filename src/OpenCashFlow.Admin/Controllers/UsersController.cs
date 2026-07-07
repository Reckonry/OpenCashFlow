using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenCashFlow.Admin.Services;
using OpenCashFlow.Contracts.DTOs.Admin;
using System.Text.Json;

namespace OpenCashFlow.Admin.Controllers
{
    [Authorize(Policy = "InstanceAdmin")]
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
        /// GET /Admin/Users - User list with filters
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
        /// GET /Admin/Users/Details/{id} - User details
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _userManagementService.GetUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "User not found";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }

        /// <summary>
        /// GET /Admin/Users/Create - User creation form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new User_Create_DTO());
        }

        /// <summary>
        /// POST /Admin/Users/Create - Create user
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
                TempData["Error"] = result.Message ?? "Error creating user";
                await LoadViewData();
                return View(dto);
            }

            TempData["Success"] = "User created successfully";
            return RedirectToAction(nameof(Details), new { id = result.Data?.UserID });
        }

        /// <summary>
        /// GET /Admin/Users/Edit/{id} - User edit form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _userManagementService.GetUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "User not found";
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
        /// POST /Admin/Users/Edit/{id} - Update user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User_Update_DTO dto)
        {
            if (id != dto.UserID)
            {
                TempData["Error"] = "User ID does not match";
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
                TempData["Error"] = result.Message ?? "Error updating user";
                await LoadViewData();
                return View(dto);
            }

            TempData["Success"] = "User updated successfully";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Lock/{id} - Lock user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(Guid id, DateTime? lockoutEnd = null)
        {
            var result = await _userManagementService.LockUserAsync(id, lockoutEnd, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Error locking user";
            }
            else
            {
                TempData["Success"] = "User locked successfully";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Unlock/{id} - Unlock user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(Guid id)
        {
            var result = await _userManagementService.UnlockUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Error unlocking user";
            }
            else
            {
                TempData["Success"] = "User unlocked successfully";
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
                TempData["Error"] = "Invalid password";
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
                TempData["Error"] = result.Message ?? "Error resetting password";
            }
            else
            {
                TempData["Success"] = "Password reset successfully";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/UpdateRoles/{id} - Update roles
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
                TempData["Error"] = result.Message ?? "Error updating roles";
            }
            else
            {
                TempData["Success"] = "Roles updated successfully";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /Admin/Users/Delete/{id} - Delete user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userManagementService.DeleteUserAsync(id, HttpContext.RequestAborted);

            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Error deleting user";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task LoadViewData()
        {
            // Load companies
            var companiesResponse = await _companyAPIService.GetCompaniesAsync();
            ViewBag.Companies = companiesResponse.Data ?? new List<OpenCashFlow.Contracts.DTOs.Company_Detail_DTO>();

            // Load roles
            var rolesResponse = await _userManagementService.GetRolesAsync(HttpContext.RequestAborted);
            ViewBag.Roles = rolesResponse.Data ?? new List<Role_DTO>();
        }

        #endregion
    }
}
