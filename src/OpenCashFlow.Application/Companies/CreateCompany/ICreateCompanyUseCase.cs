using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.CreateCompany;

public interface ICreateCompanyUseCase
{
    Task<CompanyWriteResult> ExecuteAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default);
}
