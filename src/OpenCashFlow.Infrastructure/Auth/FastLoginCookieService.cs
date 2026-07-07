using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class FastLoginCookieService : IFastLoginCookieService
{
    public string CreateCookiePayload(Guid tenantId, string companySecret)
    {
        return CookieSigner.ProtectCompanyCookie(tenantId, companySecret);
    }

    public (Guid? TenantID, bool IsValid) ValidateCookiePayload(string payload)
    {
        return CookieSigner.UnprotectCompanyCookie(payload);
    }
}
