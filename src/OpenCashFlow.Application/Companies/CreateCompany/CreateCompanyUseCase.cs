using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.CreateCompany;

public sealed class CreateCompanyUseCase(ICompanyReader companyReader, ICompanyWriter companyWriter) : ICreateCompanyUseCase
{
    public async Task<CompanyWriteResult> ExecuteAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CurrentUserID == Guid.Empty)
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Current user id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.CompanyName))
        {
            return new CompanyWriteResult(CompanyWriteStatus.ValidationFailed, Message: "Company name is required.");
        }

        var companyName = command.CompanyName.Trim();
        if (await companyReader.ExistsByNameAsync(companyName, cancellationToken: cancellationToken))
        {
            return new CompanyWriteResult(CompanyWriteStatus.DuplicateCompanyName, Message: "Company name already exists.");
        }

        var tin = Normalize(command.Tin);
        if (tin is not null && await companyReader.ExistsByTinAsync(tin, cancellationToken: cancellationToken))
        {
            return new CompanyWriteResult(CompanyWriteStatus.DuplicateTin, Message: "Tax identification number already exists.");
        }

        command.CompanyName = companyName;
        command.Tin = tin;
        var company = await companyWriter.CreateAsync(command, cancellationToken);
        return new CompanyWriteResult(CompanyWriteStatus.Success, company);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
