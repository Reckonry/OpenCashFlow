using global::Shared.DTOs;

namespace OpenCashFlow.WebApp.Services
{
    public partial class PaymentAPIService
    {
        public async Task<IEnumerable<Payment_List_DTO>?> GetRecentPaymentsAsync(int count = 5, CancellationToken cancellationToken = default)
        {
            var filters = new Payment_Filter_DTO
            {
                FromDate = null,
                ToDate = null,
                SortBy = "DateIns",
                Desc = true,
                Page = 1,
                PageSize = Math.Clamp(count, 1, 50)
            };

            var response = await _httpClient.PostAsJsonAsync("v1/Payments/", filters, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Errore HTTP POST /v1/Payments: {Status} - {Error}", response.StatusCode, errorText);
                return Enumerable.Empty<Payment_List_DTO>();
            }

            var payments = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_List_DTO>?>(cancellationToken: cancellationToken);
            return payments?.Take(count) ?? Enumerable.Empty<Payment_List_DTO>();
        }
    }
}

