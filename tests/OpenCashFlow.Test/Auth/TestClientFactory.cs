using OpenCashFlow.Test.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

public class TestClientFactory
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public TestClientFactory(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _services = factory.Services;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(Guid userId, string role = "User")
    {
        var token = await JwtTokenGenerator.GenerateTokenAsync(_services, userId, role);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client;
    }

    public HttpClient CreateAnonymousClient()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        return _client;
    }
}
