using global::Shared.DTOs.Admin;
using global::Shared.Models;
using System.Net.Http.Json;

namespace OpenCashFlow.Admin.Services
{
    public class UserManagementAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserManagementAPIService> _logger;

        public UserManagementAPIService(IHttpClientFactory httpClientFactory, ILogger<UserManagementAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        /// <summary>
        /// Ottiene lista paginata di utenti con filtri
        /// </summary>
        public async Task<ApiResponse<object>> GetUsersAsync(User_Filter_DTO filters, CancellationToken cancellationToken = default)
        {
            try
            {
                var queryParams = BuildQueryString(filters);
                var response = await _httpClient.GetAsync($"/v1/Admin/Users?{queryParams}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP GET /v1/Admin/Users: {Status} - {Error}", response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante la richiesta: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli utenti");
                return new ApiResponse<object>(false, "Errore nel recupero degli utenti", null);
            }
        }

        /// <summary>
        /// Ottiene dettaglio utente
        /// </summary>
        public async Task<ApiResponse<User_Detail_DTO>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/Admin/Users/{userId}", cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new ApiResponse<User_Detail_DTO>(false, "Utente non trovato", null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP GET /v1/Admin/Users/{UserId}: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<User_Detail_DTO>(false, $"Errore durante la richiesta: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<User_Detail_DTO>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<User_Detail_DTO>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dell'utente {UserId}", userId);
                return new ApiResponse<User_Detail_DTO>(false, "Errore nel recupero dell'utente", null);
            }
        }

        /// <summary>
        /// Crea nuovo utente
        /// </summary>
        public async Task<ApiResponse<User_Detail_DTO>> CreateUserAsync(User_Create_DTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/v1/Admin/Users", dto, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP POST /v1/Admin/Users: {Status} - {Error}", response.StatusCode, errorText);
                    return new ApiResponse<User_Detail_DTO>(false, $"Errore durante la creazione: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<User_Detail_DTO>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<User_Detail_DTO>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione dell'utente");
                return new ApiResponse<User_Detail_DTO>(false, "Errore nella creazione dell'utente", null);
            }
        }

        /// <summary>
        /// Aggiorna utente esistente
        /// </summary>
        public async Task<ApiResponse<User_Detail_DTO>> UpdateUserAsync(Guid userId, User_Update_DTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/v1/Admin/Users/{userId}", dto, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP PUT /v1/Admin/Users/{UserId}: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<User_Detail_DTO>(false, $"Errore durante l'aggiornamento: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<User_Detail_DTO>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<User_Detail_DTO>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento dell'utente {UserId}", userId);
                return new ApiResponse<User_Detail_DTO>(false, "Errore nell'aggiornamento dell'utente", null);
            }
        }

        /// <summary>
        /// Blocca accesso utente
        /// </summary>
        public async Task<ApiResponse<object>> LockUserAsync(Guid userId, DateTime? lockoutEnd = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/v1/Admin/Users/{userId}/Lock", new { LockoutEnd = lockoutEnd }, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP POST /v1/Admin/Users/{UserId}/Lock: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante il blocco: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel blocco dell'utente {UserId}", userId);
                return new ApiResponse<object>(false, "Errore nel blocco dell'utente", null);
            }
        }

        /// <summary>
        /// Sblocca accesso utente
        /// </summary>
        public async Task<ApiResponse<object>> UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/v1/Admin/Users/{userId}/Unlock", null, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP POST /v1/Admin/Users/{UserId}/Unlock: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante lo sblocco: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nello sblocco dell'utente {UserId}", userId);
                return new ApiResponse<object>(false, "Errore nello sblocco dell'utente", null);
            }
        }

        /// <summary>
        /// Reset password utente
        /// </summary>
        public async Task<ApiResponse<object>> ResetPasswordAsync(Guid userId, User_ResetPassword_DTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/v1/Admin/Users/{userId}/ResetPassword", dto, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP POST /v1/Admin/Users/{UserId}/ResetPassword: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante il reset password: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel reset password dell'utente {UserId}", userId);
                return new ApiResponse<object>(false, "Errore nel reset password", null);
            }
        }

        /// <summary>
        /// Aggiorna ruoli utente
        /// </summary>
        public async Task<ApiResponse<object>> UpdateUserRolesAsync(Guid userId, User_Roles_DTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/v1/Admin/Users/{userId}/Roles", dto, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP PUT /v1/Admin/Users/{UserId}/Roles: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante l'aggiornamento ruoli: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento ruoli dell'utente {UserId}", userId);
                return new ApiResponse<object>(false, "Errore nell'aggiornamento ruoli", null);
            }
        }

        /// <summary>
        /// Ottiene lista ruoli disponibili
        /// </summary>
        public async Task<ApiResponse<List<Role_DTO>>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/v1/Admin/Users/Roles", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP GET /v1/Admin/Users/Roles: {Status} - {Error}", response.StatusCode, errorText);
                    return new ApiResponse<List<Role_DTO>>(false, $"Errore durante la richiesta: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<Role_DTO>>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<List<Role_DTO>>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei ruoli");
                return new ApiResponse<List<Role_DTO>>(false, "Errore nel recupero dei ruoli", null);
            }
        }

        /// <summary>
        /// Elimina utente
        /// </summary>
        public async Task<ApiResponse<object>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/v1/Admin/Users/{userId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Errore HTTP DELETE /v1/Admin/Users/{UserId}: {Status} - {Error}", userId, response.StatusCode, errorText);
                    return new ApiResponse<object>(false, $"Errore durante l'eliminazione: {response.StatusCode}", null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: cancellationToken);
                return apiResponse ?? new ApiResponse<object>(false, "Risposta non valida dal server.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione dell'utente {UserId}", userId);
                return new ApiResponse<object>(false, "Errore nell'eliminazione dell'utente", null);
            }
        }

        #region Helper Methods

        private string BuildQueryString(User_Filter_DTO filters)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(filters.Search))
                queryParams.Add($"Search={Uri.EscapeDataString(filters.Search)}");

            if (filters.TenantID.HasValue)
                queryParams.Add($"TenantID={filters.TenantID.Value}");

            if (!string.IsNullOrWhiteSpace(filters.Role))
                queryParams.Add($"Role={Uri.EscapeDataString(filters.Role)}");

            if (filters.IsActive.HasValue)
                queryParams.Add($"IsActive={filters.IsActive.Value}");

            if (filters.IsLocked.HasValue)
                queryParams.Add($"IsLocked={filters.IsLocked.Value}");

            if (filters.EmailConfirmed.HasValue)
                queryParams.Add($"EmailConfirmed={filters.EmailConfirmed.Value}");

            if (filters.TwoFactorEnabled.HasValue)
                queryParams.Add($"TwoFactorEnabled={filters.TwoFactorEnabled.Value}");

            if (filters.CreatedFrom.HasValue)
                queryParams.Add($"CreatedFrom={filters.CreatedFrom.Value:O}");

            if (filters.CreatedTo.HasValue)
                queryParams.Add($"CreatedTo={filters.CreatedTo.Value:O}");

            if (filters.LastLoginFrom.HasValue)
                queryParams.Add($"LastLoginFrom={filters.LastLoginFrom.Value:O}");

            if (filters.LastLoginTo.HasValue)
                queryParams.Add($"LastLoginTo={filters.LastLoginTo.Value:O}");

            queryParams.Add($"Page={filters.Page}");
            queryParams.Add($"PageSize={filters.PageSize}");
            queryParams.Add($"SortBy={Uri.EscapeDataString(filters.SortBy)}");
            queryParams.Add($"SortDescending={filters.SortDescending}");

            return string.Join("&", queryParams);
        }

        #endregion
    }
}
