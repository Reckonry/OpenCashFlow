using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Admin;
using global::Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrator")]
    [Route("v{version:apiVersion}/Admin/Users")]
    [ApiVersion("1.0")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserManagementService userManagementService, ILogger<UsersController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        /// <summary>
        /// GET /v1/Admin/Users - Lista paginata di utenti con filtri
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetUsers([FromQuery] User_Filter_DTO filters, CancellationToken cancellationToken)
        {
            try
            {
                filters ??= new User_Filter_DTO();
                var (users, totalCount) = await _userManagementService.GetUsersAsync(filters, cancellationToken);

                return Ok(new ApiResponse<object>(true, string.Empty, new
                {
                    users,
                    totalCount,
                    page = filters.Page,
                    pageSize = filters.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize)
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new ApiResponse<object>(false, "Errore nel recupero degli utenti", null));
            }
        }

        /// <summary>
        /// GET /v1/Admin/Users/{id} - Dettaglio utente
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<User_Detail_DTO>>> GetUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManagementService.GetUserDetailAsync(id, cancellationToken);
                return Ok(new ApiResponse<User_Detail_DTO>(true, string.Empty, user));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<User_Detail_DTO>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new ApiResponse<User_Detail_DTO>(false, "Errore nel recupero dell'utente", null));
            }
        }

        /// <summary>
        /// POST /v1/Admin/Users - Crea nuovo utente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<User_Detail_DTO>>> CreateUser([FromBody] User_Create_DTO dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var user = await _userManagementService.CreateUserAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, new ApiResponse<User_Detail_DTO>(true, "Utente creato con successo", user));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating user");
                return BadRequest(new ApiResponse<User_Detail_DTO>(false, ex.Message, null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Company not found");
                return NotFound(new ApiResponse<User_Detail_DTO>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ApiResponse<User_Detail_DTO>(false, "Errore nella creazione dell'utente", null));
            }
        }

        /// <summary>
        /// PUT /v1/Admin/Users/{id} - Aggiorna utente
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<User_Detail_DTO>>> UpdateUser(Guid id, [FromBody] User_Update_DTO dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (dto.UserID != id)
            {
                return BadRequest(new ApiResponse<User_Detail_DTO>(false, "ID utente non corrispondente", null));
            }

            try
            {
                var user = await _userManagementService.UpdateUserAsync(dto, cancellationToken);
                return Ok(new ApiResponse<User_Detail_DTO>(true, "Utente aggiornato con successo", user));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<User_Detail_DTO>(false, ex.Message, null));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation updating user {UserId}", id);
                return BadRequest(new ApiResponse<User_Detail_DTO>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new ApiResponse<User_Detail_DTO>(false, "Errore nell'aggiornamento dell'utente", null));
            }
        }

        /// <summary>
        /// POST /v1/Admin/Users/{id}/Lock - Blocca accesso utente
        /// </summary>
        [HttpPost("{id:guid}/Lock")]
        public async Task<ActionResult<ApiResponse<object>>> LockUser(Guid id, [FromBody] LockUserRequest_DTO? request, CancellationToken cancellationToken)
        {
            try
            {
                await _userManagementService.LockUserAsync(id, request?.LockoutEnd, cancellationToken);
                return Ok(new ApiResponse<object>(true, "Utente bloccato con successo", null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Errore nel blocco dell'utente", null));
            }
        }

        /// <summary>
        /// POST /v1/Admin/Users/{id}/Unlock - Sblocca accesso utente
        /// </summary>
        [HttpPost("{id:guid}/Unlock")]
        public async Task<ActionResult<ApiResponse<object>>> UnlockUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _userManagementService.UnlockUserAsync(id, cancellationToken);
                return Ok(new ApiResponse<object>(true, "Utente sbloccato con successo", null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Errore nello sblocco dell'utente", null));
            }
        }

        /// <summary>
        /// POST /v1/Admin/Users/{id}/ResetPassword - Reset password utente
        /// </summary>
        [HttpPost("{id:guid}/ResetPassword")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(Guid id, [FromBody] User_ResetPassword_DTO dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (dto.UserID != id)
            {
                return BadRequest(new ApiResponse<object>(false, "ID utente non corrispondente", null));
            }

            try
            {
                await _userManagementService.ResetPasswordAsync(dto, cancellationToken);
                return Ok(new ApiResponse<object>(true, "Password reimpostata con successo", null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Errore nel reset della password", null));
            }
        }

        /// <summary>
        /// PUT /v1/Admin/Users/{id}/Roles - Gestione ruoli utente
        /// </summary>
        [HttpPut("{id:guid}/Roles")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRoles(Guid id, [FromBody] User_Roles_DTO dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (dto.UserID != id)
            {
                return BadRequest(new ApiResponse<object>(false, "ID utente non corrispondente", null));
            }

            try
            {
                await _userManagementService.UpdateUserRolesAsync(dto, cancellationToken);
                return Ok(new ApiResponse<object>(true, "Ruoli aggiornati con successo", null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Errore nell'aggiornamento dei ruoli", null));
            }
        }

        /// <summary>
        /// GET /v1/Admin/Users/Roles - Lista ruoli disponibili
        /// </summary>
        [HttpGet("Roles")]
        public async Task<ActionResult<ApiResponse<List<Role_DTO>>>> GetRoles(CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _userManagementService.GetRolesAsync(cancellationToken);
                return Ok(new ApiResponse<List<Role_DTO>>(true, string.Empty, roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new ApiResponse<List<Role_DTO>>(false, "Errore nel recupero dei ruoli", null));
            }
        }

        /// <summary>
        /// DELETE /v1/Admin/Users/{id} - Elimina utente
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _userManagementService.DeleteUserAsync(id, cancellationToken);
                return Ok(new ApiResponse<object>(true, "Utente eliminato con successo", null));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found", id);
                return NotFound(new ApiResponse<object>(false, ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Errore nell'eliminazione dell'utente", null));
            }
        }
    }

    // Helper DTO for lock endpoint
    public class LockUserRequest_DTO
    {
        public DateTime? LockoutEnd { get; set; }
    }
}
