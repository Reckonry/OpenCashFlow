using OpenCashFlow.Application.Companies.CreateCompany;
using OpenCashFlow.Application.Companies.DeleteCompany;
using OpenCashFlow.Application.Companies.GetCompanies;
using OpenCashFlow.Application.Companies.GetCompany;
using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.UpdateCompany;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Companies;

namespace OpenCashFlow.API.Services
{
    public partial class CompanyService : ICompanyService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IGetCompanyUseCase _getCompanyUseCase;
        private readonly IGetCompaniesUseCase _getCompaniesUseCase;
        private readonly ICreateCompanyUseCase _createCompanyUseCase;
        private readonly IUpdateCompanyUseCase _updateCompanyUseCase;
        private readonly IDeleteCompanyUseCase _deleteCompanyUseCase;

        public CompanyService(
            IAuthenticationService authenticationService,
            IGetCompanyUseCase getCompanyUseCase,
            IGetCompaniesUseCase getCompaniesUseCase,
            ICreateCompanyUseCase createCompanyUseCase,
            IUpdateCompanyUseCase updateCompanyUseCase,
            IDeleteCompanyUseCase deleteCompanyUseCase,
            OpenCashFlow.Application.Companies.Invoices.IGetCompanyInvoicesUseCase getCompanyInvoicesUseCase,
            OpenCashFlow.Application.Companies.Invoices.IGetCompanyInvoiceDetailUseCase getCompanyInvoiceDetailUseCase)
        {
            _authenticationService = authenticationService;
            _getCompanyUseCase = getCompanyUseCase;
            _getCompaniesUseCase = getCompaniesUseCase;
            _createCompanyUseCase = createCompanyUseCase;
            _updateCompanyUseCase = updateCompanyUseCase;
            _deleteCompanyUseCase = deleteCompanyUseCase;
            _getCompanyInvoicesUseCase = getCompanyInvoicesUseCase;
            _getCompanyInvoiceDetailUseCase = getCompanyInvoiceDetailUseCase;
        }

        public async Task<Company_Detail_DTO?> GetCompanyAsync(CancellationToken cancellationToken)
        {
            var company = await _getCompanyUseCase.ExecuteAsync(_authenticationService.GetTenantID(), cancellationToken);
            return company is null ? null : MapCompany(company);
        }

        public async Task<Company_Detail_DTO?> GetCompanyAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            var company = await _getCompanyUseCase.ExecuteAsync(TenantID, cancellationToken);
            return company is null ? null : MapCompany(company);
        }

        public async Task<IEnumerable<Company_Detail_DTO>?> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _getCompaniesUseCase.ExecuteAsync(cancellationToken);
            return companies.Select(MapCompany).ToList();
        }

        public async Task<IEnumerable<Company_Detail_DTO>?> GetAllCompaniesAsync(CompanyListQuery query, CancellationToken cancellationToken)
        {
            var companies = await _getCompaniesUseCase.ExecuteAsync(query, cancellationToken);
            return companies.Select(MapCompany).ToList();
        }

        public async Task<CompanyWriteResult> CreateCompanyAsync(Company_Detail_DTO model, CancellationToken cancellationToken)
        {
            var result = await _createCompanyUseCase.ExecuteAsync(MapWriteCommand(model, Guid.Empty), cancellationToken);
            return MapResult(result);
        }

        public async Task<CompanyWriteResult> UpdateCompanyAsync(Guid tenantId, Company_Detail_DTO model, CancellationToken cancellationToken)
        {
            var result = await _updateCompanyUseCase.ExecuteAsync(MapWriteCommand(model, tenantId), cancellationToken);
            return MapResult(result);
        }

        public Task<CompanyWriteResult> DeleteCompanyAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            return _deleteCompanyUseCase.ExecuteAsync(
                new CompanyDeleteCommand(tenantId, _authenticationService.GetUserID()),
                cancellationToken);
        }

        private CompanyWriteCommand MapWriteCommand(Company_Detail_DTO model, Guid tenantId)
        {
            return new CompanyWriteCommand
            {
                TenantID = tenantId == Guid.Empty ? model.TenantID : tenantId,
                CompanyName = model.CompanyName,
                MaxUsers = model.MaxUsers == 0 ? null : model.MaxUsers,
                Avatar = model.Avatar,
                BusinessCategory = model.BusinessCategory,
                EstimatedAnnualRevenue = model.EstimatedAnnualRevenue,
                BusinessHours = model.BusinessHours,
                Website = model.Website,
                SocialLinks = model.SocialLinks,
                InternalRating = model.InternalRating,
                PriorityLevel = model.PriorityLevel,
                VATRates = model.VATRates,
                VAT = model.VAT,
                SDI = model.SDI,
                Tin = model.TIN ?? model.NIN,
                AttorneyName = model.AttorneyName,
                AttorneyMiddleName = model.AttorneyMiddleName,
                AttorneySurname = model.AttorneySurname,
                IBAN = model.IBAN,
                BIC = model.BIC,
                SWIFT = model.SWIFT,
                PreferredPaymentMethod = model.PreferredPaymentMethod,
                MonthlyExpenseLimit = model.MonthlyExpenseLimit,
                BaseDiscountPercentage = model.BaseDiscountPercentage,
                StartingContract = model.StartingContract == default ? null : model.StartingContract,
                EndingContract = model.EndingContract == default ? null : model.EndingContract,
                LicenseType = model.LicenseType,
                GdprConsent = model.GdprConsent,
                GdprConsentDate = model.GdprConsentDate,
                ContractAcepted = model.ContractAcepted,
                ContractVersion = model.ContractVersion,
                ContractAcceptedDate = model.ContractAcceptedDate,
                DefaultCurrency = model.DefaultCurrency,
                DefaultTimezone = model.DefaultTimezone,
                DefaultLanguage = model.DefaultLanguage,
                DefaultCountry = model.DefaultCountry,
                IsActive = model.IsActive,
                StatusID = model.StatusID == Guid.Empty ? null : model.StatusID,
                CurrentUserID = _authenticationService.GetUserID()
            };
        }

        private static CompanyWriteResult MapResult(CompanyWriteResult result)
        {
            return result.Company is null
                ? result
                : result with { Company = result.Company };
        }

        private static Company_Detail_DTO MapCompany(CompanyResult company)
        {
            return new Company_Detail_DTO
            {
                TenantID = company.TenantID,
                CompanyName = company.CompanyName,
                SysCompanyName = company.SysCompanyName,
                MaxUsers = company.MaxUsers,
                Avatar = company.Avatar,
                BusinessCategory = company.BusinessCategory,
                EstimatedAnnualRevenue = company.EstimatedAnnualRevenue,
                BusinessHours = company.BusinessHours,
                Website = company.Website,
                SocialLinks = company.SocialLinks,
                InternalRating = company.InternalRating,
                PriorityLevel = company.PriorityLevel,
                VATRates = company.VATRates,
                VAT = company.VAT,
                SDI = company.SDI,
                NIN = company.NIN,
                AttorneyName = company.AttorneyName,
                AttorneyMiddleName = company.AttorneyMiddleName,
                AttorneySurname = company.AttorneySurname,
                IBAN = company.IBAN,
                BIC = company.BIC,
                SWIFT = company.SWIFT,
                PreferredPaymentMethod = company.PreferredPaymentMethod,
                MonthlyExpenseLimit = company.MonthlyExpenseLimit,
                BaseDiscountPercentage = company.BaseDiscountPercentage,
                StartingContract = company.StartingContract,
                EndingContract = company.EndingContract,
                LicenseType = company.LicenseType,
                GdprConsent = company.GdprConsent,
                GdprConsentDate = company.GdprConsentDate,
                ContractAcepted = company.ContractAcepted,
                ContractVersion = company.ContractVersion,
                ContractAcceptedDate = company.ContractAcceptedDate,
                DefaultCurrency = company.DefaultCurrency,
                DefaultTimezone = company.DefaultTimezone,
                DefaultLanguage = company.DefaultLanguage,
                DefaultCountry = company.DefaultCountry,
                IsActive = company.IsActive,
                StatusID = company.StatusID,
                IsDeleted = company.IsDeleted,
                IsDeletedBy = company.IsDeletedBy,
                IsDeletedWhy = company.IsDeletedWhy,
                DateDeleted = company.DateDeleted,
                CreatedBy = company.CreatedBy,
                DateIns = company.DateIns,
                EditedBy = company.EditedBy,
                DateEdit = company.DateEdit
            };
        }

        private static CompanyInvoiceItemDto MapInvoiceItem(CompanyInvoiceItemResult item)
        {
            return new CompanyInvoiceItemDto
            {
                TenantID = item.TenantID,
                InvoiceItemID = item.InvoiceItemID,
                InvoiceID = item.InvoiceID,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxPercent = item.TaxPercent
            };
        }

        private static Company_Invoice_List_DTO MapInvoiceList(CompanyInvoiceListItem invoice)
        {
            return new Company_Invoice_List_DTO
            {
                TenantID = invoice.TenantID,
                InvoiceID = invoice.InvoiceID,
                CompanyName = invoice.CompanyName,
                BillingAddressLine1 = invoice.BillingAddressLine1,
                BillingAddressLine2 = invoice.BillingAddressLine2,
                BillingCity = invoice.BillingCity,
                BillingState = invoice.BillingState,
                BillingPostalCode = invoice.BillingPostalCode,
                BillingCountry = invoice.BillingCountry,
                TaxIdentificationNumber = invoice.TaxIdentificationNumber,
                ContactName = invoice.ContactName,
                ContactEmail = invoice.ContactEmail,
                ContactPhone = invoice.ContactPhone,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                PaymentDate = invoice.PaymentDate,
                Status = invoice.Status,
                TotalAmount = invoice.TotalAmount,
                TotalTaxes = invoice.TotalTaxes,
                AmountPaid = invoice.AmountPaid,
                Notes = invoice.Notes,
                Items = invoice.Items.Select(MapInvoiceItem).ToList(),
                IvoicePath = invoice.IvoicePath,
                PaymentMethod = invoice.PaymentMethod,
                TransactionID = invoice.TransactionID,
                PaymentProcessor = invoice.PaymentProcessor,
                CreatedBy = invoice.CreatedBy,
                DateIns = invoice.DateIns,
                EditedBy = invoice.EditedBy,
                DateEdit = invoice.DateEdit
            };
        }

        private static Company_Invoices_Detail_DTO MapInvoiceDetail(CompanyInvoiceDetailResult invoice)
        {
            return new Company_Invoices_Detail_DTO
            {
                TenantID = invoice.TenantID,
                InvoiceID = invoice.InvoiceID,
                CompanyName = invoice.CompanyName,
                BillingAddressLine1 = invoice.BillingAddressLine1,
                BillingAddressLine2 = invoice.BillingAddressLine2,
                BillingCity = invoice.BillingCity,
                BillingState = invoice.BillingState,
                BillingPostalCode = invoice.BillingPostalCode,
                BillingCountry = invoice.BillingCountry,
                TaxIdentificationNumber = invoice.TaxIdentificationNumber,
                ContactName = invoice.ContactName,
                ContactEmail = invoice.ContactEmail,
                ContactPhone = invoice.ContactPhone,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                PaymentDate = invoice.PaymentDate,
                Status = invoice.Status,
                TotalAmount = invoice.TotalAmount,
                TotalTaxes = invoice.TotalTaxes,
                AmountPaid = invoice.AmountPaid,
                Notes = invoice.Notes,
                Items = invoice.Items.Select(MapInvoiceItem).ToList(),
                IvoicePath = invoice.IvoicePath,
                PaymentMethod = invoice.PaymentMethod,
                TransactionID = invoice.TransactionID,
                PaymentProcessor = invoice.PaymentProcessor,
                CreatedBy = invoice.CreatedBy,
                DateIns = invoice.DateIns,
                EditedBy = invoice.EditedBy,
                DateEdit = invoice.DateEdit
            };
        }
    }
}
