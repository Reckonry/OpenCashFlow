using Microsoft.AspNetCore.Identity.Data;
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
}

// Model for the login response
public class LoginResponse
{
    public string? Token { get; set; }
}
