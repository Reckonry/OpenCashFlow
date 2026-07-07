using OpenCashFlow.Application.Employees.DeleteEmployee;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Application.Employees.UpdateMyProfile;

namespace OpenCashFlow.Application.Tests.Employees;

public sealed class EmployeeUseCaseTests
{
    [Fact]
    public async Task DeleteEmployee_WithCurrentUser_Throws()
    {
        var userId = Guid.NewGuid();
        var useCase = new DeleteEmployeeUseCase(new FakeEmployeeReader(), new FakeEmployeeWriter());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(userId, Guid.NewGuid(), userId));
    }

    [Fact]
    public async Task DeleteEmployee_WithMissingEmployee_ReturnsFalse()
    {
        var useCase = new DeleteEmployeeUseCase(new FakeEmployeeReader(), new FakeEmployeeWriter());

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMyProfile_WithEmptyUser_Throws()
    {
        var useCase = new UpdateMyProfileUseCase(new FakeEmployeeWriter());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(Guid.Empty, new EmployeeProfileUpdate { UserFirstName = "A" }));
    }

    private sealed class FakeEmployeeReader : IEmployeeReader
    {
        public Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EmployeeListItem>>([]);
        }

        public Task<EmployeeDetailResult?> GetEmployeeByIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EmployeeDetailResult?>(null);
        }

        public Task<EmployeeCredentialSnapshot?> GetEmployeeCredentialAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EmployeeCredentialSnapshot?>(null);
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> EmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class FakeEmployeeWriter : IEmployeeWriter
    {
        public Task<EmployeeDetailResult?> CreateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EmployeeDetailResult?>(null);
        }

        public Task<bool> UpdateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SoftDeleteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task UpdateMyProfileAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdatePinHashAsync(Guid userId, string pinHash, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
