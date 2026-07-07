using OpenCashFlow.Application.Companies.GetCompanies;
using OpenCashFlow.Application.Companies.GetCompany;
using OpenCashFlow.Application.Companies.Models;
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

        public CompanyService(
            IAuthenticationService authenticationService,
            IGetCompanyUseCase getCompanyUseCase,
            IGetCompaniesUseCase getCompaniesUseCase,
            OpenCashFlow.Application.Companies.Invoices.IGetCompanyInvoicesUseCase getCompanyInvoicesUseCase,
            OpenCashFlow.Application.Companies.Invoices.IGetCompanyInvoiceDetailUseCase getCompanyInvoiceDetailUseCase)
        {
            _authenticationService = authenticationService;
            _getCompanyUseCase = getCompanyUseCase;
            _getCompaniesUseCase = getCompaniesUseCase;
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
