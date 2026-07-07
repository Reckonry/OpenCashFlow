using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.GetCompanies;

public interface IGetCompaniesUseCase
{
    Task<IReadOnlyList<CompanyResult>> ExecuteAsync(CancellationToken cancellationToken = default);
}
