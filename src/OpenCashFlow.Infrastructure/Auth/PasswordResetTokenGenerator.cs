using OpenCashFlow.Application.Auth.Ports;
using System.Text;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class PasswordResetTokenGenerator : IPasswordResetTokenGenerator
{
    public string GenerateToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    public string EncodeToken(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string DecodeTokenOrPassthrough(string token)
    {
        try
        {
            var value = token.Replace('-', '+').Replace('_', '/');
            value += (value.Length % 4) switch
            {
                2 => "==",
                3 => "=",
                _ => string.Empty
            };
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch
        {
            return token;
        }
    }
}
