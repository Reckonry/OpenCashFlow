using OpenCashFlow.Application.Employees.Models;

namespace OpenCashFlow.Application.Employees.UpdateMyProfile;

public interface IUpdateMyProfileUseCase
{
    Task ExecuteAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default);
}
