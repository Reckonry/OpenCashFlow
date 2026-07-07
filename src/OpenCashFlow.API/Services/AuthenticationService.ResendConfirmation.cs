using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Auth.AccountConfirmation;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        public async Task<bool> ResendConfirmationAsync(string usernameOrEmail, CancellationToken cancellationToken)
        {
            return await _resendConfirmationUseCase.ExecuteAsync(
                new ResendConfirmationCommand(usernameOrEmail),
                cancellationToken);
        }
    }
}
