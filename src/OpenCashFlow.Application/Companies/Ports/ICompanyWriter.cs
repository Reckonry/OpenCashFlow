using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.Ports;

public interface ICompanyWriter
{
    Task<CompanyResult> CreateAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default);
    Task<CompanyResult?> UpdateAsync(CompanyWriteCommand command, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(CompanyDeleteCommand command, CancellationToken cancellationToken = default);
}
