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

    public async Task<string> FastLoginAsync(string pin, string FLCookieValue)
    {
        // Assicuriamoci che ilCookie not found “FLCookie” sia già presente nell’HttpClient.
        // Se usi HttpClientHandler con CookieContainer, quel cookie verrà automaticamente inviato.

        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/fastlogin", new
        {
            Pin = pin,
            FLCookieValue
        });

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"FastLogin fallito: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            throw new HttpRequestException($"Errore durante il fast login: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result?.Token ?? throw new HttpRequestException("Token non ricevuto dal fast login");
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
