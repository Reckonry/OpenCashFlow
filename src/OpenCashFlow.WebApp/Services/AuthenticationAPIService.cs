using OpenCashFlow.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public partial class AuthenticationAPIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationAPIService> _logger;

    public AuthenticationAPIService(IHttpClientFactory httpClientFactory, ILogger<AuthenticationAPIService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("API-Client");
        _logger = logger;
    }

    public async Task<string> ForgotPasswordAsync(string email)
    {
        // Send the object with the Email property directly, without a wrapper
        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/forgot-password", new
        {
            Email = email
        });

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Ripristino password fallito: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            throw new HttpRequestException($"Errore ripristino password: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        return result?.Message ?? "Email inviata con successo";
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(object resetRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/reset-password", resetRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Reset password failed: {Status} - {Body}", response.StatusCode, errorBody);

                return new ApiResponse<string>(false, $"Errore reset password: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>(cancellationToken: cancellationToken);

            if (result is not null && !string.IsNullOrEmpty(result.Message))
                return new ApiResponse<string>(true, result.Message);

            return new ApiResponse<string>(false, "Risposta inattesa dal server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during password reset");
            return new ApiResponse<string>(false, "Errore di connessione al server");
        }
    }

    public async Task<ApiResponse<object>> ChangePasswordRequiredAsync(string newPassword)
    {
        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/change-password-required", new
        {
            NewPassword = newPassword
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("ChangePasswordRequired failed: {Status} - {Body}", response.StatusCode, body);
            return new ApiResponse<object>(false, $"Errore durante il cambio password: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result ?? new ApiResponse<object>(false, "Risposta non valida dal server");
    }

    public async Task<ApiResponse<object>> KeepCurrentPasswordAsync()
    {
        var response = await _httpClient.PostAsync("/v1/Authentication/keep-current-password", null);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("KeepCurrentPassword failed: {Status} - {Body}", response.StatusCode, body);
            return new ApiResponse<object>(false, $"Errore durante l'operazione: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return result ?? new ApiResponse<object>(false, "Risposta non valida dal server");
    }

    public async Task<(bool IsValid, bool IsExpired)> ValidateResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/Authentication/validate-reset-token?token={Uri.EscapeDataString(token)}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token validation failed: {Status}", response.StatusCode);
                return (false, false);
            }

            var result = await response.Content.ReadFromJsonAsync<ValidateResetTokenResponse>(cancellationToken: cancellationToken);

            if (result != null)
            {
                return (result.IsValid, result.IsExpired);
            }

            return (false, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token validation");
            return (false, false);
        }
    }

    /// <summary>
    /// Regenerates the JWT token with updated user claims from the database.
    /// Call this after profile updates to refresh the navbar without requiring logout.
    /// </summary>
    public async Task<ApiResponse<AuthResult>> RegenerateTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync("/v1/Authentication/regenerate", null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Token regeneration failed: {Status} - {Body}", response.StatusCode, body);
                return new ApiResponse<AuthResult>(false, $"Errore durante il refresh del token: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>(cancellationToken: cancellationToken);
            return result ?? new ApiResponse<AuthResult>(false, "Risposta non valida dal server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during token regeneration");
            return new ApiResponse<AuthResult>(false, "Errore di connessione al server");
        }
    }
}

// Model for the login response
public class LoginResponse
{
    public string? Token { get; set; }
}
