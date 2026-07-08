using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.UpdateCompany;

public interface IUpdateCompanyUseCase
{
    Task<CompanyWriteResult> ExecuteAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default);
}
