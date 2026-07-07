using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Companies;

public sealed class CompanyReader(ApplicationDbContext db) : ICompanyReader
{
    public async Task<CompanyResult?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var company = await db.Company_DS.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantID == tenantId, cancellationToken);

        return company is null ? null : MapCompany(company);
    }

    public async Task<IReadOnlyList<CompanyResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await db.Company_DS.AsNoTracking().ToListAsync(cancellationToken);
        return companies.Select(MapCompany).ToList();
    }

    public async Task<IReadOnlyList<CompanyInvoiceListItem>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var invoices = await db.Company_Invoice_DS.AsNoTracking()
            .Where(p => p.TenantID == tenantId)
            .ToListAsync(cancellationToken);

        return invoices.Select(MapInvoiceList).ToList();
    }

    public async Task<CompanyInvoiceDetailResult?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var invoice = await db.Company_Invoice_DS.AsNoTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(p => p.TenantID == tenantId && p.InvoiceID == invoiceId, cancellationToken);

        return invoice is null ? null : MapInvoiceDetail(invoice);
    }

    private static CompanyResult MapCompany(Company company)
    {
        return new CompanyResult
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
            NIN = company.TIN,
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

    private static CompanyInvoiceListItem MapInvoiceList(Company_Invoice invoice)
    {
        return new CompanyInvoiceListItem
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

    private static CompanyInvoiceDetailResult MapInvoiceDetail(Company_Invoice invoice)
    {
        return new CompanyInvoiceDetailResult
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

    private static CompanyInvoiceItemResult MapInvoiceItem(Company_Invoice_Item item)
    {
        return new CompanyInvoiceItemResult
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
}
