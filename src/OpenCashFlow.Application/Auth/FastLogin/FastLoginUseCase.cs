using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.FastLogin;

public sealed class FastLoginUseCase(
    IAuthUserReader userReader,
    IFastLoginCookieService cookieService,
    IJwtTokenIssuer jwtTokenIssuer) : IFastLoginUseCase
{
    public async Task<FastLoginResult> ExecuteAsync(FastLoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CookieValue))
        {
            return new FastLoginResult(false, "Cookie not found", null, null, null, null);
        }

        var (tenantId, validCookie) = cookieService.ValidateCookiePayload(command.CookieValue);
        if (!validCookie || tenantId is null)
        {
            return new FastLoginResult(false, "Invalid signature", null, null, null, null);
        }

        if (!await userReader.CompanyExistsAsync(tenantId.Value, cancellationToken))
        {
            return new FastLoginResult(false, "Company not found", null, null, tenantId, null);
        }

        var user = await userReader.GetByTenantAndPinAsync(tenantId.Value, command.Pin, cancellationToken);
        if (user is null)
        {
            return new FastLoginResult(false, "User not found", null, null, tenantId, null);
        }

        var claims = await userReader.GetUserClaimsAsync(user.UserID, cancellationToken);
        var token = jwtTokenIssuer.IssueToken(user, tenantId.Value, claims, command.SessionMinutes);
        return new FastLoginResult(true, string.Empty, token, user.UserID, tenantId, user.UserName);
    }
}
