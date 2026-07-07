using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.FastLogin;

public sealed class GenerateFastLoginCookieUseCase(
    IAuthUserReader userReader,
    IAuthPasswordVerifier passwordVerifier,
    IFastLoginCookieService cookieService) : IGenerateFastLoginCookieUseCase
{
    public async Task<FastLoginCookieResult> ExecuteAsync(GenerateFastLoginCookieCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.Enabled)
        {
            return new FastLoginCookieResult(true, null, null);
        }

        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
        {
            return new FastLoginCookieResult(false, null, AuthFailure.InvalidCredentials);
        }

        var user = await userReader.GetByUsernameEmailOrPhoneAsync(command.Username, cancellationToken);
        if (user is null || !passwordVerifier.VerifyPassword(command.Password, user.PasswordSalt, user.PasswordHash))
        {
            return new FastLoginCookieResult(false, null, AuthFailure.InvalidCredentials);
        }

        var tenantId = await userReader.GetUserTenantIdAsync(user.UserID, cancellationToken);
        if (tenantId is null)
        {
            return new FastLoginCookieResult(false, null, AuthFailure.InternalError);
        }

        var secret = await userReader.GetCompanySecretAsync(tenantId.Value, cancellationToken);
        if (string.IsNullOrWhiteSpace(secret))
        {
            return new FastLoginCookieResult(false, null, AuthFailure.InternalError);
        }

        return new FastLoginCookieResult(true, cookieService.CreateCookiePayload(tenantId.Value, secret), null);
    }
}
