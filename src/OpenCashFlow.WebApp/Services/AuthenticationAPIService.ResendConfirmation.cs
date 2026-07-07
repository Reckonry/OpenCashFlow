public partial class AuthenticationAPIService
{
    public async Task<bool> ResendConfirmationAsync(string username)
    {
        var payload = new { Username = username };
        var response = await _httpClient.PostAsJsonAsync("/v1/Authentication/resend-confirmation", payload);
        // Always OK by design; treat non-success as false
        return response.IsSuccessStatusCode;
    }
}

