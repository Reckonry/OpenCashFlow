using OpenCashFlow.App.Services;
using Microsoft.AspNetCore.Mvc;
using global::Shared.Models.DTOs;
using global::Shared.Models;

namespace OpenCashFlow.App.Controllers
{
    public partial class CompanyController : Controller
    {
        // Payment Methods
        [HttpGet("Payment_Methods"), ActionName("Company_Payment_Methods")]
        public async Task<IActionResult> Company_Payment_Methods()
        {
            var items = await _paymentAPIService.GetPaymentMethodsAsync();
            return View("~/Views/Company/Company_Payment_Methods.cshtml", items ?? Enumerable.Empty<Payment_Method_List_DTO>());
        }

        [HttpGet("Payment_Methods/Create"), ActionName("Company_Payment_Method_Create")]
        public IActionResult Company_Payment_Method_Create()
        {
            return View("~/Views/Company/Company_Payment_Method_Create.cshtml", new Payment_Method_Create_DTO()
            {
                PaymentMethodName = string.Empty,
                Visible = true,
                DisplayOrder = 999
            });
        }

        [HttpPost("Payment_Methods/Create"), ActionName("Company_Payment_Method_Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_Method_Create_Post([FromForm] Payment_Method_Create_DTO model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Company/Company_Payment_Method_Create.cshtml", model);

            var result = await _paymentAPIService.AddPaymentMethodAsync(model);
            if (result.Success == false)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Errore creazione metodo pagamento");
                return View("~/Views/Company/Company_Payment_Method_Create.cshtml", model);
            }
            return Redirect("/Company/Payment_Methods");
        }

        [HttpGet("Payment_Methods/Edit/{PaymentMethodID:guid}"), ActionName("Company_Payment_Method_Edit")]
        public async Task<IActionResult> Company_Payment_Method_Edit(Guid PaymentMethodID)
        {
            var resp = await _paymentAPIService.GetPaymentMethodAsync(PaymentMethodID);
            if (resp?.Success != true || resp.Data == null)
                return NotFound();

            var detail = resp.Data;
            var model = new Payment_Method_Update_DTO
            {
                PaymentMethodID = detail.PaymentMethodID,
                PaymentMethodName = detail.PaymentMethodName,
                PaymentMethodDescription = detail.PaymentMethodDescription,
                PaymentMethodIcon = detail.PaymentMethodIcon,
                Visible = detail.Visible,
                DisplayOrder = detail.DisplayOrder,
                TenantID = detail.TenantID,
                IsDeleted = detail.IsDeleted,
                IsDeletedBy = detail.IsDeletedBy,
                IsDeletedWhy = detail.IsDeletedWhy,
                DateDeleted = detail.DateDeleted,
                CreatedBy = detail.CreatedBy,
                DateIns = detail.DateIns,
                EditedBy = detail.EditedBy,
                DateEdit = detail.DateEdit
            };
            return View("~/Views/Company/Company_Payment_Method_Edit.cshtml", model);
        }

        [HttpPost("Payment_Methods/Edit"), ActionName("Company_Payment_Method_Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_Method_Edit_Post([FromForm] Payment_Method_Update_DTO model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Company/Company_Payment_Method_Edit.cshtml", model);

            var result = await _paymentAPIService.UpdatePaymentAsync(model);
            if (result.Success == false)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Errore aggiornamento metodo pagamento");
                return View("~/Views/Company/Company_Payment_Method_Edit.cshtml", model);
            }
            return Redirect("/Company/Payment_Methods");
        }

        [HttpPost("Payment_Methods/Delete"), ActionName("Company_Payment_Method_Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_Method_Delete([FromForm] Guid PaymentMethodID)
        {
            if (PaymentMethodID == Guid.Empty)
                return Redirect("/Company/Payment_Methods");

            var result = await _paymentAPIService.DeletePaymentMethodAsync(PaymentMethodID);
            if (result.Success == false)
            {
                TempData["Error"] = result.Message ?? "Errore eliminazione metodo di pagamento";
            }
            return Redirect("/Company/Payment_Methods");
        }

        [HttpGet("Payment_Methods/Delete")]
        public async Task<IActionResult> Company_Payment_Method_Delete_Get([FromQuery] Guid? PaymentMethodID)
        {
            if (PaymentMethodID.HasValue && PaymentMethodID.Value != Guid.Empty)
            {
                var result = await _paymentAPIService.DeletePaymentMethodAsync(PaymentMethodID.Value);
                if (!result.Success)
                    TempData["Error"] = result.Message ?? "Errore eliminazione metodo di pagamento";
            }
            return Redirect("/Company/Payment_Methods");
        }

        // Document Types
        [HttpGet("Payment_DocumentTypes"), ActionName("Company_Payment_DocumentTypes")]
        public async Task<IActionResult> Company_Payment_DocumentTypes()
        {
            var items = await _paymentAPIService.GetDocumentTypesAsync();
            return View("~/Views/Company/Company_Payment_DocumentTypes.cshtml", items ?? Enumerable.Empty<Payment_DocumentType_List_DTO>());
        }

        [HttpGet("Payment_DocumentTypes/Create"), ActionName("Company_Payment_DocumentType_Create")]
        public IActionResult Company_Payment_DocumentType_Create()
        {
            return View("~/Views/Company/Company_Payment_DocumentType_Create.cshtml", new Payment_DocumentType_Create_DTO()
            {
                DocumentTypeName = string.Empty,
                Visible = true,
                DisplayOrder = 999
            });
        }

        [HttpPost("Payment_DocumentTypes/Create"), ActionName("Company_Payment_DocumentType_Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_DocumentType_Create_Post([FromForm] Payment_DocumentType_Create_DTO model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Company/Company_Payment_DocumentType_Create.cshtml", model);

            var result = await _paymentAPIService.AddDocumentTypeAsync(model);
            if (result.Success == false)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Errore creazione tipo documento");
                return View("~/Views/Company/Company_Payment_DocumentType_Create.cshtml", model);
            }
            return Redirect("/Company/Payment_DocumentTypes");
        }

        [HttpGet("Payment_DocumentTypes/Edit/{DocumentTypeID:guid}"), ActionName("Company_Payment_DocumentType_Edit")]
        public async Task<IActionResult> Company_Payment_DocumentType_Edit(Guid DocumentTypeID)
        {
            var resp = await _paymentAPIService.GetDocumentTypeAsync(DocumentTypeID);
            if (resp?.Success != true || resp.Data == null)
                return NotFound();

            var detail = resp.Data;
            var model = new Payment_DocumentType_Update_DTO
            {
                DocumentTypeID = detail.DocumentTypeID,
                DocumentTypeName = detail.DocumentTypeName,
                DocumentTypeDescription = detail.DocumentTypeDescription,
                DocumentTypeIcon = detail.DocumentTypeIcon,
                Visible = detail.Visible,
                DisplayOrder = detail.DisplayOrder,
                TenantID = detail.TenantID,
                IsDeleted = detail.IsDeleted,
                IsDeletedBy = detail.IsDeletedBy,
                IsDeletedWhy = detail.IsDeletedWhy,
                DateDeleted = detail.DateDeleted,
                CreatedBy = detail.CreatedBy,
                DateIns = detail.DateIns,
                EditedBy = detail.EditedBy,
                DateEdit = detail.DateEdit
            };
            return View("~/Views/Company/Company_Payment_DocumentType_Edit.cshtml", model);
        }

        [HttpPost("Payment_DocumentTypes/Edit"), ActionName("Company_Payment_DocumentType_Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_DocumentType_Edit_Post([FromForm] Payment_DocumentType_Update_DTO model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Company/Company_Payment_DocumentType_Edit.cshtml", model);

            var result = await _paymentAPIService.UpdateDocumentTypeAsync(model);
            if (result.Success == false)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Errore aggiornamento tipo documento");
                return View("~/Views/Company/Company_Payment_DocumentType_Edit.cshtml", model);
            }
            return Redirect("/Company/Payment_DocumentTypes");
        }

        [HttpPost("Payment_DocumentTypes/Delete"), ActionName("Company_Payment_DocumentType_Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Company_Payment_DocumentType_Delete([FromForm] Guid DocumentTypeID)
        {
            if (DocumentTypeID == Guid.Empty)
                return Redirect("/Company/Payment_DocumentTypes");

            var result = await _paymentAPIService.DeleteDocumentTypeAsync(DocumentTypeID);
            if (result.Success == false)
            {
                TempData["Error"] = result.Message ?? "Errore eliminazione tipo documento";
            }
            return Redirect("/Company/Payment_DocumentTypes");
        }

        // Fallback GET con supporto a query string
        [HttpGet("Payment_DocumentTypes/Delete")]
        public async Task<IActionResult> Company_Payment_DocumentType_Delete_Get([FromQuery] Guid? DocumentTypeID)
        {
            if (DocumentTypeID.HasValue && DocumentTypeID.Value != Guid.Empty)
            {
                var result = await _paymentAPIService.DeleteDocumentTypeAsync(DocumentTypeID.Value);
                if (!result.Success)
                    TempData["Error"] = result.Message ?? "Errore eliminazione tipo documento";
            }
            return Redirect("/Company/Payment_DocumentTypes");
        }
    }
}
