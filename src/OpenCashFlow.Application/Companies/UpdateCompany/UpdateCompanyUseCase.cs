using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.UpdateCompany;

public sealed class UpdateCompanyUseCase(ICompanyReader companyReader, ICompanyWriter companyWriter) : IUpdateCompanyUseCase
{
    public async Task<CompanyWriteResult> ExecuteAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TenantID == Guid.Empty)
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Tenant id is required.");
        }

        if (command.CurrentUserID == Guid.Empty)
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Current user id is required.");
        }

        var existing = await companyReader.GetByIdAsync(command.TenantID, cancellationToken);
        if (existing is null || existing.IsDeleted)
        {
            return new CompanyWriteResult(CompanyWriteStatus.NotFound, Message: "Company not found.");
        }

        if (command.CompanyName is not null && string.IsNullOrWhiteSpace(command.CompanyName))
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Company name is required.");
        }

        var companyName = Normalize(command.CompanyName);
        if (companyName is not null && await companyReader.ExistsByNameAsync(companyName, command.TenantID, cancellationToken))
        {
            return new CompanyWriteResult(CompanyWriteStatus.DuplicateCompanyName, Message: "Company name already exists.");
        }

        var tin = Normalize(command.Tin);
        if (tin is not null && await companyReader.ExistsByTinAsync(tin, command.TenantID, cancellationToken))
        {
            return new CompanyWriteResult(CompanyWriteStatus.DuplicateTin, Message: "Tax identification number already exists.");
        }

        command.CompanyName = companyName ?? existing.CompanyName;
        command.Tin = tin;
        var updated = await companyWriter.UpdateAsync(command, cancellationToken);
        return updated is null
            ? new CompanyWriteResult(CompanyWriteStatus.NotFound, Message: "Company not found.")
            : new CompanyWriteResult(CompanyWriteStatus.Success, updated);
    }

    private static string? Normalize(string? value)
    {
        return value is null ? null : value.Trim();
    }
}
