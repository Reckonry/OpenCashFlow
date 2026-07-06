using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Calendar;
using OpenCashFlow.Application.Payments.CreatePayment;
using OpenCashFlow.Application.Payments.DeletePayment;
using OpenCashFlow.Application.Payments.DocumentTypes;
using OpenCashFlow.Application.Payments.GetPaymentDetail;
using OpenCashFlow.Application.Payments.GetPayments;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Application.Payments.Reports;
using OpenCashFlow.Application.Payments.UpdatePayment;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService : IPaymentService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICreatePaymentOrchestrator _createPaymentOrchestrator;
        private readonly IUpdatePaymentOrchestrator _updatePaymentOrchestrator;
        private readonly IDeletePaymentOrchestrator _deletePaymentOrchestrator;
        private readonly IGetPaymentsUseCase _getPaymentsUseCase;
        private readonly IGetPaymentDetailUseCase _getPaymentDetailUseCase;
        private readonly IGetPaymentReportsUseCase _getPaymentReportsUseCase;
        private readonly IGetPaymentCalendarUseCase _getPaymentCalendarUseCase;
        private readonly IGetPaymentMethodsUseCase _getPaymentMethodsUseCase;
        private readonly IGetPaymentMethodDetailUseCase _getPaymentMethodDetailUseCase;
        private readonly ICreatePaymentMethodUseCase _createPaymentMethodUseCase;
        private readonly IUpdatePaymentMethodUseCase _updatePaymentMethodUseCase;
        private readonly IDeletePaymentMethodUseCase _deletePaymentMethodUseCase;
        private readonly IGetDocumentTypesUseCase _getDocumentTypesUseCase;
        private readonly IGetDocumentTypeDetailUseCase _getDocumentTypeDetailUseCase;
        private readonly ICreateDocumentTypeUseCase _createDocumentTypeUseCase;
        private readonly IUpdateDocumentTypeUseCase _updateDocumentTypeUseCase;
        private readonly IDeleteDocumentTypeUseCase _deleteDocumentTypeUseCase;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IAuthenticationService authenticationService,
            ICreatePaymentOrchestrator createPaymentOrchestrator,
            IUpdatePaymentOrchestrator updatePaymentOrchestrator,
            IDeletePaymentOrchestrator deletePaymentOrchestrator,
            IGetPaymentsUseCase getPaymentsUseCase,
            IGetPaymentDetailUseCase getPaymentDetailUseCase,
            IGetPaymentReportsUseCase getPaymentReportsUseCase,
            IGetPaymentCalendarUseCase getPaymentCalendarUseCase,
            IGetPaymentMethodsUseCase getPaymentMethodsUseCase,
            IGetPaymentMethodDetailUseCase getPaymentMethodDetailUseCase,
            ICreatePaymentMethodUseCase createPaymentMethodUseCase,
            IUpdatePaymentMethodUseCase updatePaymentMethodUseCase,
            IDeletePaymentMethodUseCase deletePaymentMethodUseCase,
            IGetDocumentTypesUseCase getDocumentTypesUseCase,
            IGetDocumentTypeDetailUseCase getDocumentTypeDetailUseCase,
            ICreateDocumentTypeUseCase createDocumentTypeUseCase,
            IUpdateDocumentTypeUseCase updateDocumentTypeUseCase,
            IDeleteDocumentTypeUseCase deleteDocumentTypeUseCase,
            ILogger<PaymentService> logger)
        {
            _authenticationService = authenticationService;
            _createPaymentOrchestrator = createPaymentOrchestrator;
            _updatePaymentOrchestrator = updatePaymentOrchestrator;
            _deletePaymentOrchestrator = deletePaymentOrchestrator;
            _getPaymentsUseCase = getPaymentsUseCase;
            _getPaymentDetailUseCase = getPaymentDetailUseCase;
            _getPaymentReportsUseCase = getPaymentReportsUseCase;
            _getPaymentCalendarUseCase = getPaymentCalendarUseCase;
            _getPaymentMethodsUseCase = getPaymentMethodsUseCase;
            _getPaymentMethodDetailUseCase = getPaymentMethodDetailUseCase;
            _createPaymentMethodUseCase = createPaymentMethodUseCase;
            _updatePaymentMethodUseCase = updatePaymentMethodUseCase;
            _deletePaymentMethodUseCase = deletePaymentMethodUseCase;
            _getDocumentTypesUseCase = getDocumentTypesUseCase;
            _getDocumentTypeDetailUseCase = getDocumentTypeDetailUseCase;
            _createDocumentTypeUseCase = createDocumentTypeUseCase;
            _updateDocumentTypeUseCase = updateDocumentTypeUseCase;
            _deleteDocumentTypeUseCase = deleteDocumentTypeUseCase;
            _logger = logger;
        }

        public async Task<IEnumerable<Payment_List_DTO>?> GetAllPaymentsAsync(Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
            filters ??= new Payment_Filter_DTO();
            var query = ToPaymentListQuery(filters, _authenticationService.GetTenantID());
            var payments = await _getPaymentsUseCase.ExecuteAsync(new GetPaymentsQuery(query), cancellationToken);
            return payments.Select(ToListDto).ToList();
        }

        public async Task<Payment_Detail_DTO?> GetPaymentByIdAsync(Guid PaymentID, CancellationToken cancellationToken)
        {
            var payment = await _getPaymentDetailUseCase.ExecuteAsync(
                new GetPaymentDetailQuery(PaymentID, _authenticationService.GetTenantID()),
                cancellationToken);

            return payment == null ? null : ToDetailDto(payment);
        }

        public async Task<Payment_Create_DTO?> AddPaymentAsync(Payment_Create_DTO payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;

            var command = new CreatePaymentCommand(
                payment.PaymentID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                payment.RequestId,
                Convert.ToDecimal(payment.Amount),
                payment.EntryType,
                payment.PaymentMethodID,
                payment.DocumentTypeID,
                payment.DateIns,
                payment.Description);

            var result = await _createPaymentOrchestrator.ExecuteAsync(command, cancellationToken);
            return result == null ? null : ToCreateDto(result.Payment);
        }

        public async Task<Payment_Update_DTO?> UpdatePaymentAsync(Payment_Detail_DTO payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;

            var command = new UpdatePaymentCommand(
                payment.PaymentID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID(),
                Convert.ToDecimal(payment.Amount),
                payment.EntryType,
                payment.PaymentMethodID,
                payment.DocumentTypeID,
                payment.DateIns,
                payment.Description);

            var result = await _updatePaymentOrchestrator.ExecuteAsync(command, cancellationToken);
            return result == null ? null : ToUpdateDto(result.Payment);
        }

        public async Task<bool> DeletePaymentAsync(Guid PaymentID, CancellationToken cancellationToken)
        {
            var command = new DeletePaymentCommand(
                PaymentID,
                _authenticationService.GetTenantID(),
                _authenticationService.GetUserID());

            var result = await _deletePaymentOrchestrator.ExecuteAsync(command, cancellationToken);
            return result.Deleted;
        }

        private static Payment_Create_DTO ToCreateDto(PaymentSnapshot payment)
        {
            return new Payment_Create_DTO
            {
                PaymentID = payment.PaymentId,
                TenantID = payment.TenantId,
                RequestId = payment.RequestId,
                Amount = Convert.ToDouble(payment.Amount),
                EntryType = payment.EntryType,
                PaymentMethodID = payment.PaymentMethodId,
                DocumentTypeID = payment.DocumentTypeId,
                Description = payment.Description,
                UserID = payment.UserId,
                DateIns = payment.DateIns
            };
        }

        private static Payment_Update_DTO ToUpdateDto(PaymentSnapshot payment)
        {
            return new Payment_Update_DTO
            {
                PaymentID = payment.PaymentId,
                TenantID = payment.TenantId,
                Amount = Convert.ToDouble(payment.Amount),
                EntryType = payment.EntryType,
                PaymentMethodID = payment.PaymentMethodId,
                DocumentTypeID = payment.DocumentTypeId,
                Description = payment.Description,
                UserID = payment.UserId,
                DateIns = payment.DateIns
            };
        }

        private static PaymentListQuery ToPaymentListQuery(Payment_Filter_DTO filters, Guid tenantId)
        {
            return new PaymentListQuery(
                tenantId,
                filters.PaymentID,
                filters.EntryType,
                filters.PaymentMethodID,
                filters.DocumentTypeID,
                filters.UserID,
                filters.FromDate,
                filters.ToDate,
                filters.MinAmount,
                filters.MaxAmount,
                filters.Description,
                filters.IsDeleted,
                filters.SortBy,
                filters.Desc,
                filters.Page,
                filters.PageSize);
        }

        private static Payment_List_DTO ToListDto(PaymentListItem payment)
        {
            return new Payment_List_DTO
            {
                PaymentID = payment.PaymentId,
                TenantID = payment.TenantId,
                Amount = payment.Amount,
                EntryType = payment.EntryType,
                PaymentMethodID = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethodName,
                DocumentTypeID = payment.DocumentTypeId,
                DocumentTypeName = payment.DocumentTypeName,
                Description = payment.Description,
                UserID = payment.UserId,
                EmployeeFullName = payment.EmployeeFullName,
                IsDeleted = payment.IsDeleted,
                IsDeletedBy = payment.IsDeletedBy,
                IsDeletedWhy = payment.IsDeletedWhy,
                DateDeleted = payment.DateDeleted,
                CreatedBy = payment.CreatedBy,
                DateIns = payment.DateIns,
                EditedBy = payment.EditedBy,
                DateEdit = payment.DateEdit
            };
        }

        private static Payment_Detail_DTO ToDetailDto(PaymentDetailResult payment)
        {
            return new Payment_Detail_DTO
            {
                PaymentID = payment.PaymentId,
                TenantID = payment.TenantId,
                Amount = payment.Amount,
                EntryType = payment.EntryType,
                PaymentMethodID = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethodName,
                DocumentTypeID = payment.DocumentTypeId,
                DocumentTypeName = payment.DocumentTypeName,
                Description = payment.Description,
                UserID = payment.UserId,
                EmployeeFullName = payment.EmployeeFullName,
                IsDeleted = payment.IsDeleted,
                IsDeletedBy = payment.IsDeletedBy,
                IsDeletedWhy = payment.IsDeletedWhy,
                DateDeleted = payment.DateDeleted,
                CreatedBy = payment.CreatedBy,
                DateIns = payment.DateIns,
                EditedBy = payment.EditedBy,
                DateEdit = payment.DateEdit
            };
        }

        private static Payment_DailyPayments ToDailyPaymentModel(DailyPaymentResult payment)
        {
            return new Payment_DailyPayments
            {
                DailyPaymentsID = payment.DailyPaymentsId,
                TenantID = payment.TenantId,
                CashDate = payment.CashDate,
                Total = payment.Total,
                DateIns = payment.DateIns
            };
        }

        private static Payment_Method_List_DTO ToPaymentMethodListDto(PaymentMethodListItem method)
        {
            return new Payment_Method_List_DTO
            {
                PaymentMethodID = method.PaymentMethodId,
                TenantID = method.TenantId,
                PaymentMethodName = method.Name,
                PaymentMethodDescription = method.Description,
                PaymentMethodIcon = method.Icon,
                Visible = method.Visible,
                DisplayOrder = method.DisplayOrder,
                IsDeleted = method.IsDeleted,
                IsDeletedBy = method.IsDeletedBy,
                IsDeletedWhy = method.IsDeletedWhy,
                DateDeleted = method.DateDeleted,
                CreatedBy = method.CreatedBy,
                DateIns = method.DateIns,
                EditedBy = method.EditedBy,
                DateEdit = method.DateEdit
            };
        }

        private static Payment_Method_Detail_DTO ToPaymentMethodDetailDto(PaymentMethodResult method)
        {
            return new Payment_Method_Detail_DTO
            {
                PaymentMethodID = method.PaymentMethodId,
                TenantID = method.TenantId,
                PaymentMethodName = method.Name,
                PaymentMethodDescription = method.Description,
                PaymentMethodIcon = method.Icon,
                Visible = method.Visible,
                DisplayOrder = method.DisplayOrder,
                IsDeleted = method.IsDeleted,
                IsDeletedBy = method.IsDeletedBy,
                IsDeletedWhy = method.IsDeletedWhy,
                DateDeleted = method.DateDeleted,
                CreatedBy = method.CreatedBy,
                DateIns = method.DateIns,
                EditedBy = method.EditedBy,
                DateEdit = method.DateEdit
            };
        }

        private static Payment_Method_Create_DTO ToPaymentMethodCreateDto(PaymentMethodResult method)
        {
            return new Payment_Method_Create_DTO
            {
                PaymentMethodID = method.PaymentMethodId,
                TenantID = method.TenantId,
                PaymentMethodName = method.Name,
                PaymentMethodDescription = method.Description,
                PaymentMethodIcon = method.Icon,
                Visible = method.Visible,
                DisplayOrder = method.DisplayOrder,
                CreatedBy = method.CreatedBy,
                DateIns = method.DateIns
            };
        }

        private static Payment_Method_Update_DTO ToPaymentMethodUpdateDto(PaymentMethodResult method)
        {
            return new Payment_Method_Update_DTO
            {
                PaymentMethodID = method.PaymentMethodId,
                TenantID = method.TenantId,
                PaymentMethodName = method.Name,
                PaymentMethodDescription = method.Description,
                PaymentMethodIcon = method.Icon,
                Visible = method.Visible,
                DisplayOrder = method.DisplayOrder,
                IsDeleted = method.IsDeleted,
                IsDeletedBy = method.IsDeletedBy,
                IsDeletedWhy = method.IsDeletedWhy,
                DateDeleted = method.DateDeleted,
                CreatedBy = method.CreatedBy,
                DateIns = method.DateIns,
                EditedBy = method.EditedBy,
                DateEdit = method.DateEdit
            };
        }

        private static Payment_DocumentType_List_DTO ToDocumentTypeListDto(DocumentTypeListItem documentType)
        {
            return new Payment_DocumentType_List_DTO
            {
                DocumentTypeID = documentType.DocumentTypeId,
                TenantID = documentType.TenantId,
                DocumentTypeName = documentType.Name,
                DocumentTypeDescription = documentType.Description,
                DocumentTypeIcon = documentType.Icon,
                Visible = documentType.Visible,
                DisplayOrder = documentType.DisplayOrder,
                IsDeleted = documentType.IsDeleted,
                IsDeletedBy = documentType.IsDeletedBy,
                IsDeletedWhy = documentType.IsDeletedWhy,
                DateDeleted = documentType.DateDeleted,
                CreatedBy = documentType.CreatedBy,
                DateIns = documentType.DateIns,
                EditedBy = documentType.EditedBy,
                DateEdit = documentType.DateEdit
            };
        }

        private static Payment_DocumentType_Detail_DTO ToDocumentTypeDetailDto(DocumentTypeResult documentType)
        {
            return new Payment_DocumentType_Detail_DTO
            {
                DocumentTypeID = documentType.DocumentTypeId,
                TenantID = documentType.TenantId,
                DocumentTypeName = documentType.Name,
                DocumentTypeDescription = documentType.Description,
                DocumentTypeIcon = documentType.Icon,
                Visible = documentType.Visible,
                DisplayOrder = documentType.DisplayOrder,
                IsDeleted = documentType.IsDeleted,
                IsDeletedBy = documentType.IsDeletedBy,
                IsDeletedWhy = documentType.IsDeletedWhy,
                DateDeleted = documentType.DateDeleted,
                CreatedBy = documentType.CreatedBy,
                DateIns = documentType.DateIns,
                EditedBy = documentType.EditedBy,
                DateEdit = documentType.DateEdit
            };
        }

        private static Payment_DocumentType_Create_DTO ToDocumentTypeCreateDto(DocumentTypeResult documentType)
        {
            return new Payment_DocumentType_Create_DTO
            {
                DocumentTypeID = documentType.DocumentTypeId,
                TenantID = documentType.TenantId,
                DocumentTypeName = documentType.Name,
                DocumentTypeDescription = documentType.Description,
                DocumentTypeIcon = documentType.Icon,
                Visible = documentType.Visible,
                DisplayOrder = documentType.DisplayOrder,
                IsDeleted = documentType.IsDeleted,
                IsDeletedBy = documentType.IsDeletedBy,
                IsDeletedWhy = documentType.IsDeletedWhy,
                DateDeleted = documentType.DateDeleted,
                CreatedBy = documentType.CreatedBy,
                DateIns = documentType.DateIns,
                EditedBy = documentType.EditedBy,
                DateEdit = documentType.DateEdit
            };
        }

        private static Payment_DocumentType_Update_DTO ToDocumentTypeUpdateDto(DocumentTypeResult documentType)
        {
            return new Payment_DocumentType_Update_DTO
            {
                DocumentTypeID = documentType.DocumentTypeId,
                TenantID = documentType.TenantId,
                DocumentTypeName = documentType.Name,
                DocumentTypeDescription = documentType.Description,
                DocumentTypeIcon = documentType.Icon,
                Visible = documentType.Visible,
                DisplayOrder = documentType.DisplayOrder,
                IsDeleted = documentType.IsDeleted,
                IsDeletedBy = documentType.IsDeletedBy,
                IsDeletedWhy = documentType.IsDeletedWhy,
                DateDeleted = documentType.DateDeleted,
                CreatedBy = documentType.CreatedBy,
                DateIns = documentType.DateIns,
                EditedBy = documentType.EditedBy,
                DateEdit = documentType.DateEdit
            };
        }
    }
}
