using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Employees;

namespace OpenCashFlow.WebApp.Services
{
    public class EmployeeAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmployeeAPIService> _logger;

        public EmployeeAPIService(IHttpClientFactory httpClientFactory, ILogger<EmployeeAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<Employee_List_DTO>?>> GetEmployeesAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/Employees/");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<IEnumerable<Employee_List_DTO>?>(false, "Dipenenti non trovati.");

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Employees/: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<IEnumerable<Employee_List_DTO>?>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Employee_List_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<IEnumerable<Employee_List_DTO>?>(false, "Risposta non valida dal server.");

            return new ApiResponse<IEnumerable<Employee_List_DTO>?>(true, "", apiResponse);
        }

        public async Task<ApiResponse<Employee_Detail_DTO?>> GetEmployeeByIDAsync(Guid UserID)
        {
            var response = await _httpClient.GetAsync($"/v1/Employee/{UserID}");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Employee_Detail_DTO?>(false, "Dipenente non trovato.");

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Employees/: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<Employee_Detail_DTO?>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<Employee_Detail_DTO>();
            if (apiResponse == null)
                return new ApiResponse<Employee_Detail_DTO?>(false, "Risposta non valida dal server.");

            return new ApiResponse<Employee_Detail_DTO?>(true, "", apiResponse);
        }

        public async Task<ApiResponse<Employee_Detail_DTO?>> CreateEmployeeAsync(Employee_Create_DTO model, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/Employee", model);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Employee: {Status} - {Error}", response.StatusCode, errorText);

                // Try to parse structured error response
                string errorMessage = "Errore durante la creazione del dipendente.";
                try
                {
                    using var document = System.Text.Json.JsonDocument.Parse(errorText);
                    if (document.RootElement.TryGetProperty("error", out var errorProperty))
                    {
                        errorMessage = errorProperty.GetString() ?? errorMessage;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // If JSON parsing fails, use the raw error text if it's reasonably short
                    if (!string.IsNullOrWhiteSpace(errorText) && errorText.Length < 200)
                        errorMessage = errorText;
                }

                // Return specific status code information
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return new ApiResponse<Employee_Detail_DTO?>(false, errorMessage);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return new ApiResponse<Employee_Detail_DTO?>(false, errorMessage);
                }

                return new ApiResponse<Employee_Detail_DTO?>(false, errorMessage);
            }

            var createdEmployee = await response.Content.ReadFromJsonAsync<Employee_Detail_DTO>();
            if (createdEmployee == null)
                return new ApiResponse<Employee_Detail_DTO?>(false, "Risposta non valida dal server.");

            return new ApiResponse<Employee_Detail_DTO?>(true, "Dipendente creato con successo.", createdEmployee);
        }


        public async Task<ApiResponse<Employee_Update_Response_DTO?>> UpdateEmployeeAsync(Guid UserID, Employee_Update_DTO model)
        {
            var response = await _httpClient.PutAsJsonAsync($"/v1/Employee/{UserID}", model);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Employee_Update_Response_DTO?>(false, "Dipendente non trovato.");

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /v1/Employee/{UserID}: {Status} - {Error}", UserID, response.StatusCode, errorText);
                return new ApiResponse<Employee_Update_Response_DTO?>(false, $"Errore durante la modifica: {response.StatusCode}");
            }

            var updateResponse = await response.Content.ReadFromJsonAsync<Employee_Update_Response_DTO>();
            if (updateResponse == null)
                return new ApiResponse<Employee_Update_Response_DTO?>(false, "Risposta non valida dal server.");

            return new ApiResponse<Employee_Update_Response_DTO?>(true, updateResponse.Message, updateResponse);
        }

        public async Task<ApiResponse<object>> UpdateMyProfileAsync(Employee_MyProfile_Update_DTO model)
        {
            var response = await _httpClient.PutAsJsonAsync($"/v1/Account/Profile", model);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /v1/Account/Profile: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<object>(false, $"Errore durante il salvataggio: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            return apiResponse ?? new ApiResponse<object>(true, "");
        }


        public async Task<ApiResponse<bool>> SendPasswordResetAsync(Guid userId)
        {
            var response = await _httpClient.PostAsync($"/v1/Authentication/send-password-reset/{userId}", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<bool>(false, "Utente non trovato.");

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Authentication/send-password-reset/{UserID}: {Status} - {Error}",
                    userId, response.StatusCode, errorText);

                return new ApiResponse<bool>(false, $"Errore durante l'invio: {response.StatusCode}");
            }

            return new ApiResponse<bool>(true, "Email di reset inviata correttamente.");
        }
        public async Task<ApiResponse<bool>> ResendPinAsync(Guid UserID)
        {
            var response = await _httpClient.PostAsync($"/v1/Employee/{UserID}/resend-pin", null);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<bool>(false, "Dipendente non trovato o email non configurata.");

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Employee/{UserID}/resend-pin: {Status} - {Error}", UserID, response.StatusCode, errorText);
                return new ApiResponse<bool>(false, $"Errore durante l'invio del PIN: {response.StatusCode}");
            }

            return new ApiResponse<bool>(true, "PIN inviato con successo.", true);
        }

        public async Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid UserID)
        {
            _logger.LogInformation("Attempting to delete employee with UserID: {UserID}", UserID);

            var response = await _httpClient.DeleteAsync($"/v1/Employee/{UserID}");

            _logger.LogInformation("Delete response status: {StatusCode} for UserID: {UserID}", response.StatusCode, UserID);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Employee not found for deletion: {UserID}", UserID);
                return new ApiResponse<bool>(false, "Dipendente non trovato.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("BadRequest response content for UserID {UserID}: {Content}", UserID, errorContent);

                try
                {
                    // Try to parse JSON error response
                    using var document = System.Text.Json.JsonDocument.Parse(errorContent);
                    if (document.RootElement.TryGetProperty("error", out var errorProperty))
                    {
                        var errorMessage = errorProperty.GetString();
                        _logger.LogInformation("Parsed error message for UserID {UserID}: {ErrorMessage}", UserID, errorMessage);
                        return new ApiResponse<bool>(false, errorMessage ?? "Operazione non consentita.");
                    }
                    else
                    {
                        _logger.LogWarning("JSON response for UserID {UserID} does not contain 'error' property. Root element: {RootElement}", UserID, document.RootElement.ToString());
                    }
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "Failed to parse JSON error response for UserID {UserID}. Raw content: {Content}", UserID, errorContent);
                    // If JSON parsing fails, return the raw error content if it looks like an error message
                    if (!string.IsNullOrWhiteSpace(errorContent) && errorContent.Length < 200)
                    {
                        _logger.LogInformation("Using raw error content for UserID {UserID}: {Content}", UserID, errorContent);
                        return new ApiResponse<bool>(false, errorContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse error response from DELETE /v1/Employee/{UserID}: {Content}", UserID, errorContent);
                }

                _logger.LogWarning("Returning default error message for UserID {UserID} after failed parsing", UserID);
                return new ApiResponse<bool>(false, "Operazione non consentita.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /v1/Employee/{UserID}: {Status} - {Error}", UserID, response.StatusCode, errorText);
                return new ApiResponse<bool>(false, $"Errore durante l'eliminazione: {response.StatusCode}");
            }

            _logger.LogInformation("Employee successfully deleted: {UserID}", UserID);
            return new ApiResponse<bool>(true, "Dipendente eliminato con successo.", true);
        }
    }
}
