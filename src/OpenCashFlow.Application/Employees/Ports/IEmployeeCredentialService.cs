namespace OpenCashFlow.Application.Employees.Ports;

public interface IEmployeeCredentialService
{
    string GenerateSalt();
    string HashSecret(string value, string salt);
    bool IsStrongPassword(string password);
}
