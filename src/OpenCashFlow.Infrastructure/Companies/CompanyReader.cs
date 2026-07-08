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

    public async Task<IReadOnlyList<CompanyResult>> GetAllAsync(CompanyListQuery query, CancellationToken cancellationToken = default)
    {
        var companies = db.Company_DS.AsNoTracking().AsQueryable();

        if (query.IsActive.HasValue)
        {
            companies = companies.Where(c => c.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim().ToLower();
            companies = companies.Where(c => c.CompanyName.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Tin))
        {
            var tin = query.Tin.Trim().ToLower();
            companies = companies.Where(c => c.TIN != null && c.TIN.ToLower().Contains(tin));
        }

        if (query.RevenueFrom.HasValue)
        {
            companies = companies.Where(c => c.EstimatedAnnualRevenue >= query.RevenueFrom.Value);
        }

        if (query.RevenueTo.HasValue)
        {
            companies = companies.Where(c => c.EstimatedAnnualRevenue <= query.RevenueTo.Value);
        }

        var result = await companies.ToListAsync(cancellationToken);
        return result.Select(MapCompany).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string companyName, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
    {
        var normalized = companyName.Trim().ToLower();
        return await db.Company_DS.AsNoTracking()
            .AnyAsync(c => c.CompanyName.ToLower() == normalized &&
                (!excludingTenantId.HasValue || c.TenantID != excludingTenantId.Value), cancellationToken);
    }

    public async Task<bool> ExistsByTinAsync(string tin, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
    {
        var normalized = tin.Trim().ToLower();
        return await db.Company_DS.AsNoTracking()
            .AnyAsync(c => c.TIN != null && c.TIN.ToLower() == normalized &&
                (!excludingTenantId.HasValue || c.TenantID != excludingTenantId.Value), cancellationToken);
    }

    public async Task<bool> HasActiveRelationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var hasStaff = await db.Company_Staff_DS.AsNoTracking()
            .AnyAsync(s => s.TenantID == tenantId && !s.IsDeleted, cancellationToken);
        if (hasStaff) return true;

        var hasPayments = await db.Payment_DS.AsNoTracking()
            .AnyAsync(p => p.TenantID == tenantId && !p.IsDeleted, cancellationToken);
        if (hasPayments) return true;

        return await db.Company_Invoice_DS.AsNoTracking()
            .AnyAsync(i => i.TenantID == tenantId, cancellationToken);
    }

    public async Task<(long MaxUsers, int ActiveUsers)?> GetUserLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var company = await db.Company_DS.AsNoTracking()
            .Where(c => c.TenantID == tenantId && !c.IsDeleted)
            .Select(c => new { c.MaxUsers })
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null)
        {
            return null;
        }

        var activeUsers = await db.Company_Staff_DS.AsNoTracking()
            .CountAsync(s => s.TenantID == tenantId && !s.IsDeleted, cancellationToken);

        return (company.MaxUsers, activeUsers);
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
