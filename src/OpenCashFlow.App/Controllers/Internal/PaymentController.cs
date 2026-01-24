// /Controllers/Internal/PaymentController.cs
using OpenCashFlow.App.Hubs;
using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using global::Shared.DTOs;
using global::Shared.Models;
using static global::Shared.Enums.Permissions.Customers;
using System.Net.Http.Json;

namespace OpenCashFlow.App.Controllers.Internal
{
    [Authorize]
    [Route("internal/[controller]/[action]")]
    public class PaymentController : Controller
    {
        private readonly PaymentAPIService _paymentAPIService;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(PaymentAPIService paymentAPIService, IHubContext<PaymentHub> hubContext, IHttpClientFactory httpClientFactory)
        {
            _paymentAPIService = paymentAPIService;
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult GetPaymentsList()
        {
            Payment_Filter_DTO filters = new();
            var payments = _paymentAPIService.GetPaymentsAsync(filters);  // server-side, without exposing an external API
            return PartialView("/Views/Payments/Partials/_PaymentsList", payments);
        }

        [HttpPost]
        public async Task<IActionResult> FilterPayments(Payment_Filter_DTO filters)
        {
            try
            {
                var results = await _paymentAPIService.GetPaymentsAsync(filters);
                var html = await this.RenderViewAsync("/Views/Payments/Partials/_PaymentsList.cshtml", results, partial: true);

                return Json(new { success = true, html });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error while filtering payments." });
            }
        }


        [HttpGet]
        public IActionResult CreatePaymentModal()
        {
            // Call the ViewComponent to generate the modal
            return ViewComponent("AddPaymentModal");
        }

        [HttpGet]
        public IActionResult EditPaymentModal(Guid paymentID)
        {
            // Simply forward to the ViewComponent named "EditPaymentModal"
            return ViewComponent("EditPaymentModal", new { paymentID });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(Payment_Create_DTO dto)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, render the modal again with errors
                return ViewComponent("AddPaymentModal", new { dto });
            }

            var apiResult = await _paymentAPIService.AddPaymentAsync(dto);
            if (!apiResult.Success)
                return Json(new { success = false, message = apiResult.Message });

            // ❯❯❯ Broadcast the new payment to all connected clients ❮❮❮
            var newPayment = (Payment_Detail_DTO)apiResult.Data!;
            var payload = new
            {
                PaymentID = newPayment.PaymentID,
                DateIns = newPayment.DateIns,
                Amount = newPayment.Amount,
                Description = newPayment.Description, 
                EntryType = newPayment.EntryType,                
                PaymentMethodName = newPayment.PaymentMethodName,
                PaymentMethodID = newPayment.PaymentMethodID,
                DocumentTypeName = newPayment.DocumentTypeName,
                DocumentTypeID = newPayment.DocumentTypeID,
                EmployeeFullName = newPayment.EmployeeFullName                
            };

            await _hubContext.Clients.All.SendAsync("PaymentAdded", payload);

            return Json(new { success = true, data = apiResult.Data });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayment(Payment_Update_DTO editDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid update data." });

            var apiResult = await _paymentAPIService.UpdatePaymentAsync(editDto);
            if (!apiResult.Success)
                return Json(new { success = false, message = apiResult.Message });

            // apiResult.Data is the updated Payment_Detail_DTO
            var updatedPayment = apiResult.Data!;

            // SignalR broadcast: clients will update the existing row
            await _hubContext.Clients.All.SendAsync("PaymentUpdated", updatedPayment);

            return Json(new { success = true, data = updatedPayment });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePayment(Guid paymentID)
        {
            if (paymentID == Guid.Empty)
                return BadRequest(new { success = false, message = "Invalid ID." });

            // Call the service to delete via API
            var apiResult = await _paymentAPIService.DeletePaymentAsync(paymentID);
            if (!apiResult.Success)
                return Json(new { success = false, message = apiResult.Message });

            // SignalR broadcast: clients will remove the row
            await _hubContext.Clients.All.SendAsync("PaymentDeleted", paymentID);

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCashBalance(CancellationToken cancellationToken)
        {
            var giClaim = User?.FindFirst("TenantID")?.Value;
            if (!Guid.TryParse(giClaim, out var companyId))
                return Json(new { success = false, message = "Company not found" });

            try
            {
                var client = _httpClientFactory.CreateClient("API-Client");
                var url = $"/v1/admin/cash/current?companyId={companyId}";
                using var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                    return Json(new { success = false, message = errorText });
                }

                var amount = await response.Content.ReadFromJsonAsync<decimal>(cancellationToken: cancellationToken);
                return Json(new { success = true, amount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCalendarEvents([FromBody] Payment_Filter_DTO filters)
        {
            try
            {
                var events = await _paymentAPIService.GetPaymentCalendarEventsAsync(filters);
                return Json(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
