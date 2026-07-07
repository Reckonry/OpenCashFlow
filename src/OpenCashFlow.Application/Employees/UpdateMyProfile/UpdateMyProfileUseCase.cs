using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;

namespace OpenCashFlow.Application.Employees.UpdateMyProfile;

public sealed class UpdateMyProfileUseCase(IEmployeeWriter employeeWriter) : IUpdateMyProfileUseCase
{
    public Task ExecuteAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return employeeWriter.UpdateMyProfileAsync(userId, model, cancellationToken);
    }
}
