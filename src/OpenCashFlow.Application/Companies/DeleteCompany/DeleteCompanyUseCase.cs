using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.DeleteCompany;

public sealed class DeleteCompanyUseCase(ICompanyReader companyReader, ICompanyWriter companyWriter) : IDeleteCompanyUseCase
{
    public async Task<CompanyWriteResult> ExecuteAsync(CompanyDeleteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TenantID == Guid.Empty)
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Tenant id is required.");
        }

        if (command.CurrentUserID == Guid.Empty)
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Current user id is required.");
        }

        var company = await companyReader.GetByIdAsync(command.TenantID, cancellationToken);
        if (company is null)
        {
            return new CompanyWriteResult(CompanyWriteStatus.NotFound, Message: "Company not found.");
        }

        if (company.IsDeleted)
        {
            return new CompanyWriteResult(CompanyWriteStatus.AlreadyDeleted, Message: "Company is already deleted.");
        }

        if (await companyReader.HasActiveRelationsAsync(command.TenantID, cancellationToken))
        {
            return new CompanyWriteResult(CompanyWriteStatus.HasActiveRelations, Message: "Company has active related records.");
        }

        var deleted = await companyWriter.SoftDeleteAsync(command, cancellationToken);
        return deleted
            ? new CompanyWriteResult(CompanyWriteStatus.Success)
            : new CompanyWriteResult(CompanyWriteStatus.NotFound, Message: "Company not found.");
    }
}
