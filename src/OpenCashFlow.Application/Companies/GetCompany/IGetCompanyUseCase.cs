using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.GetCompany;

public interface IGetCompanyUseCase
{
    Task<CompanyResult?> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
