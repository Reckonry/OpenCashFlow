using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.DTOs;
using System.Net.Http;

public partial class AuthenticationAPIService
{
    public async Task<Core_RegistrationResult> RegisterAsync(Register_DTO registration)
    {
        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/register", registration);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogError($"Registrazione fallita: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            throw new HttpRequestException($"Errore durante la registrazione: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<Core_RegistrationResult>();
        return result is null ? throw new HttpRequestException("Risposta di registrazione nulla dal server.") : result;
    }

    public async Task<bool> ConfirmAccount(Guid TenantID, Guid UserID, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/v1/Authentication/confirm_account/{TenantID}/{UserID}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError($"Conferma account fallita: {response.StatusCode} - {errorBody}");
            throw new HttpRequestException($"Errore conferma account: {response.StatusCode}");
        }
        var result = await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
        return result;
    }
}
