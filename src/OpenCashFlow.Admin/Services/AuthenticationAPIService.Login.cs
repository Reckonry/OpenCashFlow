using global::Shared.Models;
using global::Shared.Models.Core;

public partial class AuthenticationAPIService
{
    public async Task<ApiResponse<AuthResult>> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/login", new
        {
            Username = username,
            Password = password
        });

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError($"Login fallito: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            throw new HttpRequestException($"Errore durante il login: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();

        if (result == null || result.Data == null)
            throw new HttpRequestException("Token non ricevuto");

        return result;
    }

    public async Task LogoutAsync()
    {
        var response = await _httpClient.PostAsync("/v1/auth/logout", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Errore durante il logout: {response.StatusCode}");
        }
    }
}
