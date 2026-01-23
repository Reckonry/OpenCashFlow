using Microsoft.AspNetCore.Mvc;
using global::Shared.Models.Cash;
using System.Net.Http.Json;
using System.Linq;

namespace OpenCashFlow.App.Controllers
{
    public partial class CompanyController : Controller
    {
        [HttpGet("CashLedger"), ActionName("Company_CashLedger")]
        public IActionResult CashLedger()
        {
            return View();
        }

        [HttpGet("CashLedger/List")]
        public async Task<IActionResult> GetCashLedger(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            var giClaim = User?.FindFirst("TenantID")?.Value;
            if (!Guid.TryParse(giClaim, out var companyId))
                return Json(new { success = false, message = "Company not found" });

            try
            {
                var client = _httpClientFactory.CreateClient("API-Client");
                var url = $"/v1/admin/cash/ledger?companyId={companyId}&skip={skip}&take={Math.Clamp(take,1,200)}";
                using var response = await client.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(ct);
                    return Json(new { success = false, message = errorText });
                }
                var items = await response.Content.ReadFromJsonAsync<List<CashLedger>>(cancellationToken: ct) ?? new List<CashLedger>();

                // Build a lookup UserID -> "FirstName LastName" to display friendly names
                try
                {
                    // Collect distinct user IDs from ledger entries
                    var createdByIds = items
                        .Select(i => i.CreatedBy)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    if (createdByIds.Length > 0)
                    {
                        // Fetch employees of the current company and build dictionary
                        using var empResp = await client.GetAsync("/v1/Employees", ct);
                        if (empResp.IsSuccessStatusCode)
                        {
                            var employees = await empResp.Content.ReadFromJsonAsync<List<global::Shared.DTOs.Employees.Employee_List_DTO>>(cancellationToken: ct) ?? new();
                            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var emp in employees)
                            {
                                var key = emp.UserID.ToString();
                                var fullName = string.Join(" ", new[] { emp.UserFirstName, emp.UserLastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
                                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(fullName))
                                {
                                    map[key] = fullName;
                                }
                            }
                            // Expose the lookup to the partial via ViewData
                            ViewData["UserNames"] = map;
                        }
                    }
                }
                catch
                {
                    // Non-blocking: if lookup fails, we just keep showing the ID
                }

                var html = await this.RenderViewAsync("/Views/Company/Partials/_CashLedgerRows.cshtml", items, partial: true);
                return Json(new { success = true, html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class AdjustRequest { public decimal? Delta { get; set; } public string Reason { get; set; } = string.Empty; }

        [HttpPost("CashLedger/Adjust")]
        public async Task<IActionResult> Adjust([FromBody] AdjustRequest request, CancellationToken ct)
        {
            var giClaim = User?.FindFirst("TenantID")?.Value;
            if (!Guid.TryParse(giClaim, out var companyId))
                return Json(new { success = false, message = "Company not found" });
            if (request == null)
                return Json(new { success = false, message = "Invalid request" });
            if (!ModelState.IsValid || request.Delta == null)
                return Json(new { success = false, message = "Invalid amount" });
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Json(new { success = false, message = "Reason required" });

            var client = _httpClientFactory.CreateClient("API-Client");
            var url = "/v1/admin/cash/adjust";
            var body = new { CompanyId = companyId, Delta = request.Delta.Value, Reason = request.Reason };
            using var response = await client.PostAsJsonAsync(url, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);
                return Json(new { success = false, message = errorText });
            }
            return Json(new { success = true });
        }
    }
}
