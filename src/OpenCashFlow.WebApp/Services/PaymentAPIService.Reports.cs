
namespace OpenCashFlow.WebApp.Services
{
    public partial class PaymentAPIService
    {
        public async Task<ApiResponse<double>> GetDailyPaymentsTotalAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            var url = $"/v1/Payment/daily-payments?date={date:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Errore HTTP GET {Url}: {Status} - {Error}", url, response.StatusCode, errorText);
                return new ApiResponse<double>(false, $"HTTP {(int)response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<double>>(cancellationToken: cancellationToken);
            return apiResponse ?? new ApiResponse<double>(false, "Invalid server response.");
        }

        public async Task<ApiResponse<double>> GetMonthlyPaymentsTotalAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var url = $"/v1/Payment/monthly-payments?year={year}&month={month}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Errore HTTP GET {Url}: {Status} - {Error}", url, response.StatusCode, errorText);
                return new ApiResponse<double>(false, $"HTTP {(int)response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<double>>(cancellationToken: cancellationToken);
            return apiResponse ?? new ApiResponse<double>(false, "Invalid server response.");
        }
    }
}

