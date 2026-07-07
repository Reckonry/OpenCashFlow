using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;

namespace OpenCashFlow.Application.Auth.Login;

public sealed class LoginUseCase(
    IAuthUserReader userReader,
    IAuthPasswordVerifier passwordVerifier,
    IJwtTokenIssuer jwtTokenIssuer) : ILoginUseCase
{
    public async Task<LoginResult> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
        {
            return new LoginResult(false, null, false, AuthFailure.InvalidCredentials, null, null, command.Username);
        }

        var user = await userReader.GetByUsernameEmailOrPhoneAsync(command.Username, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordSalt) || string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordVerifier.VerifyPassword(command.Password, user.PasswordSalt, user.PasswordHash))
        {
            return new LoginResult(false, null, false, AuthFailure.InvalidCredentials, user?.UserID, null, command.Username);
        }

        if (!user.IsApproved && command.RequireActiveAccount)
        {
            return new LoginResult(false, null, false, AuthFailure.NotActive, user.UserID, null, user.UserName);
        }

        var tenantId = await userReader.GetUserTenantIdAsync(user.UserID, cancellationToken);
        if (tenantId is null)
        {
            return new LoginResult(false, null, false, AuthFailure.InternalError, user.UserID, null, user.UserName);
        }

        var claims = await userReader.GetUserClaimsAsync(user.UserID, cancellationToken);
        var token = jwtTokenIssuer.IssueToken(user, tenantId.Value, claims, command.SessionMinutes);
        return new LoginResult(true, token, user.UserMustChangePassword, null, user.UserID, tenantId, user.UserName);
    }
}
