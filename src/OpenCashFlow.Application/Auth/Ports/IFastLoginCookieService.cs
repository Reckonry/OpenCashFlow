namespace OpenCashFlow.Application.Auth.Ports;

public interface IFastLoginCookieService
{
    string CreateCookiePayload(Guid tenantId, string companySecret);
    (Guid? TenantID, bool IsValid) ValidateCookiePayload(string payload);
}
