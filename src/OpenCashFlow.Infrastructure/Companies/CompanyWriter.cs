using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Companies;

public sealed class CompanyWriter(ApplicationDbContext db) : ICompanyWriter
{
    public async Task<CompanyResult> CreateAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default)
    {
        var company = new Company
        {
            TenantID = command.TenantID == Guid.Empty ? Guid.NewGuid() : command.TenantID,
            CompanyName = command.CompanyName,
            MaxUsers = command.MaxUsers.GetValueOrDefault(50),
            CompanySecret = Guid.NewGuid().ToString("N"),
            DateIns = DateTime.UtcNow,
            CreatedBy = command.CurrentUserID
        };

        Apply(command, company, isCreate: true);

        await db.Company_DS.AddAsync(company, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return MapCompany(company);
    }

    public async Task<CompanyResult?> UpdateAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default)
    {
        var company = await db.Company_DS.FirstOrDefaultAsync(c => c.TenantID == command.TenantID && !c.IsDeleted, cancellationToken);
        if (company is null)
        {
            return null;
        }

        Apply(command, company, isCreate: false);
        company.EditedBy = command.CurrentUserID;
        company.DateEdit = DateTime.UtcNow;

        db.Company_DS.Update(company);
        await db.SaveChangesAsync(cancellationToken);
        return MapCompany(company);
    }

    public async Task<bool> SoftDeleteAsync(CompanyDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var company = await db.Company_DS.FirstOrDefaultAsync(c => c.TenantID == command.TenantID && !c.IsDeleted, cancellationToken);
        if (company is null)
        {
            return false;
        }

        company.IsDeleted = true;
        company.IsDeletedBy = command.CurrentUserID;
        company.DateDeleted = DateTime.UtcNow;
        company.IsActive = false;

        db.Company_DS.Update(company);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void Apply(CompanyWriteCommand command, Company company, bool isCreate)
    {
        company.CompanyName = command.CompanyName;
        if (command.MaxUsers.HasValue) company.MaxUsers = command.MaxUsers.Value;
        if (command.Avatar is not null) company.Avatar = command.Avatar;
        if (command.BusinessCategory is not null) company.BusinessCategory = command.BusinessCategory;
        if (command.EstimatedAnnualRevenue.HasValue) company.EstimatedAnnualRevenue = command.EstimatedAnnualRevenue;
        if (command.BusinessHours is not null) company.BusinessHours = command.BusinessHours;
        if (command.Website is not null) company.Website = command.Website;
        if (command.SocialLinks is not null) company.SocialLinks = command.SocialLinks;
        if (command.InternalRating.HasValue) company.InternalRating = command.InternalRating;
        if (command.PriorityLevel.HasValue) company.PriorityLevel = command.PriorityLevel.Value;
        if (command.VATRates.HasValue) company.VATRates = command.VATRates;
        if (command.VAT is not null) company.VAT = command.VAT;
        if (command.SDI is not null) company.SDI = command.SDI;
        if (command.Tin is not null) company.TIN = command.Tin;
        if (command.AttorneyName is not null) company.AttorneyName = command.AttorneyName;
        if (command.AttorneyMiddleName is not null) company.AttorneyMiddleName = command.AttorneyMiddleName;
        if (command.AttorneySurname is not null) company.AttorneySurname = command.AttorneySurname;
        if (command.IBAN is not null) company.IBAN = command.IBAN;
        if (command.BIC is not null) company.BIC = command.BIC;
        if (command.SWIFT is not null) company.SWIFT = command.SWIFT;
        if (command.PreferredPaymentMethod is not null) company.PreferredPaymentMethod = command.PreferredPaymentMethod;
        if (command.MonthlyExpenseLimit.HasValue) company.MonthlyExpenseLimit = command.MonthlyExpenseLimit;
        if (command.BaseDiscountPercentage.HasValue) company.BaseDiscountPercentage = command.BaseDiscountPercentage;
        if (command.StartingContract.HasValue) company.StartingContract = command.StartingContract.Value;
        if (command.EndingContract.HasValue) company.EndingContract = command.EndingContract.Value;
        if (command.LicenseType is not null) company.LicenseType = command.LicenseType;
        if (command.GdprConsent.HasValue) company.GdprConsent = command.GdprConsent.Value;
        if (command.GdprConsentDate.HasValue) company.GdprConsentDate = command.GdprConsentDate;
        if (command.ContractAcepted.HasValue) company.ContractAcepted = command.ContractAcepted.Value;
        if (command.ContractVersion is not null) company.ContractVersion = command.ContractVersion;
        if (command.ContractAcceptedDate.HasValue) company.ContractAcceptedDate = command.ContractAcceptedDate;
        if (command.DefaultCurrency is not null) company.DefaultCurrency = command.DefaultCurrency;
        if (command.DefaultTimezone is not null) company.DefaultTimezone = command.DefaultTimezone;
        if (command.DefaultLanguage is not null) company.DefaultLanguage = command.DefaultLanguage;
        if (command.DefaultCountry is not null) company.DefaultCountry = command.DefaultCountry;
        if (command.IsActive.HasValue) company.IsActive = command.IsActive.Value;
        if (command.StatusID.HasValue) company.StatusID = command.StatusID;

        if (isCreate)
        {
            company.StartingContract = command.StartingContract ?? DateTime.UtcNow;
            company.EndingContract = command.EndingContract ?? DateTime.UtcNow.AddYears(10);
            company.IsActive = command.IsActive ?? true;
        }
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
}
