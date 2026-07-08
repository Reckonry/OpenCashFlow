using OpenCashFlow.Application.Companies.Ports;
using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.GetCompanies;

public sealed class GetCompaniesUseCase(ICompanyReader companyReader) : IGetCompaniesUseCase
{
    public Task<IReadOnlyList<CompanyResult>> ExecuteAsync(CancellationToken cancellationToken = default)
        => companyReader.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<CompanyResult>> ExecuteAsync(CompanyListQuery query, CancellationToken cancellationToken = default)
        => companyReader.GetAllAsync(query, cancellationToken);
}
