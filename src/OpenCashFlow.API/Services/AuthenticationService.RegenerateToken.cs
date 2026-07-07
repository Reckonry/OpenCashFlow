using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Contracts.Core;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        public async Task<AuthResult> RegenerateTokenWithUpdatedClaimsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _authUserReader.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return new AuthResult { Success = false, ErrorType = AuthErrorType.InternalError };
            }

            var tenantId = await _authUserReader.GetUserTenantIdAsync(user.UserID, cancellationToken);
            if (tenantId == null)
            {
                return new AuthResult { Success = false, ErrorType = AuthErrorType.InternalError };
            }

            var claims = await _authUserReader.GetUserClaimsAsync(user.UserID, cancellationToken);
            var token = _jwtTokenIssuer.IssueToken(user, tenantId.Value, claims, Configuration.WebSessionDurationMinutes);

            return new AuthResult
            {
                Success = true,
                Token = token,
                RequiresPasswordChange = false
            };
        }
    }
}
