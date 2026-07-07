namespace OpenCashFlow.Application.Auth.Ports;

public interface IPasswordResetTokenGenerator
{
    string GenerateToken();
    string EncodeToken(string token);
    string DecodeTokenOrPassthrough(string token);
}
