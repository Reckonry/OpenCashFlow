using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.GetCompany;

public sealed class GetCompanyUseCase(ICompanyReader companyReader) : IGetCompanyUseCase
{
    public Task<CompanyResult?> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Task.FromResult<CompanyResult?>(null);
        }

        return companyReader.GetByIdAsync(tenantId, cancellationToken);
    }
}
