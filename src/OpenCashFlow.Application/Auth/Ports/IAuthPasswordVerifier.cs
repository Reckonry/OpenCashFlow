namespace OpenCashFlow.Application.Auth.Ports;

public interface IAuthPasswordVerifier
{
    bool VerifyPassword(string password, string salt, string expectedHash);
    string HashPassword(string password, string salt);
    string GenerateSalt();
    bool IsStrongPassword(string password);
}
