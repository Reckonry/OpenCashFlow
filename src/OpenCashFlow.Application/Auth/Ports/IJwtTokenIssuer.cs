using OpenCashFlow.Application.Auth.Models;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IJwtTokenIssuer
{
    string IssueToken(AuthenticatedUserResult user, Guid tenantId, IReadOnlyList<AuthClaimResult> customClaims, int sessionMinutes);
}
