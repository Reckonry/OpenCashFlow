namespace OpenCashFlow.Application.Auth.Ports;

public interface IUserPasswordWriter
{
    Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
    Task RemovePasswordChangeRequirementAsync(Guid userId, CancellationToken cancellationToken = default);
}
