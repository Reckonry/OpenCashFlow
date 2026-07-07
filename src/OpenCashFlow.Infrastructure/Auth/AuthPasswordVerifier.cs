using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class AuthPasswordVerifier : IAuthPasswordVerifier
{
    public bool VerifyPassword(string password, string salt, string expectedHash)
    {
        return !string.IsNullOrWhiteSpace(salt) &&
            !string.IsNullOrWhiteSpace(expectedHash) &&
            PasswordHasher.HashPasswordArgon2(password, salt) == expectedHash;
    }

    public string HashPassword(string password, string salt)
    {
        return PasswordHasher.HashPasswordArgon2(password, salt);
    }

    public string GenerateSalt()
    {
        return PasswordHasher.GenerateSalt();
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
