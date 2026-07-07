using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Infrastructure.Auth;

namespace OpenCashFlow.Infrastructure.Employees;

public sealed class EmployeeCredentialService : IEmployeeCredentialService
{
    public string GenerateSalt()
    {
        return PasswordHasher.GenerateSalt();
    }

    public string HashSecret(string value, string salt)
    {
        return PasswordHasher.HashPasswordArgon2(value, salt);
    }

    public bool IsStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
