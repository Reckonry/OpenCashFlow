using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.DeleteCompany;

public interface IDeleteCompanyUseCase
{
    Task<CompanyWriteResult> ExecuteAsync(CompanyDeleteCommand command, CancellationToken cancellationToken = default);
}
