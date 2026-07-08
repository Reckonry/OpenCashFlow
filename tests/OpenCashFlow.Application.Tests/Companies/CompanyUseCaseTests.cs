using OpenCashFlow.Application.Companies.GetCompany;
using OpenCashFlow.Application.Companies.Invoices;
using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Tests.Companies;

public sealed class CompanyUseCaseTests
{
    [Fact]
    public async Task GetCompany_WithEmptyTenant_ReturnsNull()
    {
        var reader = new FakeCompanyReader();
        var useCase = new GetCompanyUseCase(reader);

        var result = await useCase.ExecuteAsync(Guid.Empty);

        Assert.Null(result);
        Assert.False(reader.GetByIdCalled);
    }

    [Fact]
    public async Task GetInvoices_WithEmptyTenant_ReturnsEmptyList()
    {
        var reader = new FakeCompanyReader();
        var useCase = new GetCompanyInvoicesUseCase(reader);

        var result = await useCase.ExecuteAsync(Guid.Empty);

        Assert.Empty(result);
        Assert.False(reader.GetInvoicesCalled);
    }

    private sealed class FakeCompanyReader : ICompanyReader
    {
        public bool GetByIdCalled { get; private set; }
        public bool GetInvoicesCalled { get; private set; }

        public Task<CompanyResult?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            GetByIdCalled = true;
            return Task.FromResult<CompanyResult?>(new CompanyResult { CompanyName = "Test" });
        }

        public Task<IReadOnlyList<CompanyResult>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CompanyResult>>([]);
        }

        public Task<IReadOnlyList<CompanyResult>> GetAllAsync(CompanyListQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CompanyResult>>([]);
        }

        public Task<bool> ExistsByNameAsync(string companyName, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> ExistsByTinAsync(string tin, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasActiveRelationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<(long MaxUsers, int ActiveUsers)?> GetUserLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<(long MaxUsers, int ActiveUsers)?>((50, 1));
        }

        public Task<IReadOnlyList<CompanyInvoiceListItem>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            GetInvoicesCalled = true;
            return Task.FromResult<IReadOnlyList<CompanyInvoiceListItem>>([]);
        }

        public Task<CompanyInvoiceDetailResult?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CompanyInvoiceDetailResult?>(null);
        }
    }
}
