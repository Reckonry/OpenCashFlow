using System.Security.Cryptography;

namespace OpenCashFlow.WebApp.Security;

public static class CspNonce
{
    public const string HttpContextItemKey = "OpenCashFlow.CspNonce";

    public static string Create()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    }
}
