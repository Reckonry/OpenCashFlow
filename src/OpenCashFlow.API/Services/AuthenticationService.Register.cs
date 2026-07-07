using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Auth.AccountConfirmation;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Register;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        public async Task<Core_RegistrationResult> RegistrationAsync(Register_DTO registration, CancellationToken cancellationToken)
        {
            var result = await _registerUseCase.ExecuteAsync(
                new RegisterCommand(
                    registration.CompanyName,
                    registration.Email,
                    registration.Password,
                    registration.ConfirmPassword,
                    registration.AcceptPrivacyPolicy,
                    registration.FirstName,
                    registration.LastName),
                cancellationToken);

            return MapRegistrationResult(result);
        }

        public async Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken)
        {
            var result = await _confirmAccountUseCase.ExecuteAsync(
                new ConfirmAccountCommand(TenantID, UserID),
                cancellationToken);

            return result.Success;
        }

        private static Core_RegistrationResult MapRegistrationResult(RegistrationResult result)
        {
            if (result.Success)
            {
                return Core_RegistrationResult.Ok();
            }

            var error = result.ErrorType switch
            {
                RegistrationFailure.UsernameTaken => RegistrationError.UsernameTaken,
                RegistrationFailure.EmailTaken => RegistrationError.EmailTaken,
                RegistrationFailure.WeakPassword => RegistrationError.WeakPassword,
                RegistrationFailure.InvalidEmail => RegistrationError.InvalidEmail,
                RegistrationFailure.MissingRequiredFields => RegistrationError.MissingRequiredFields,
                RegistrationFailure.PasswordsDoNotMatch => RegistrationError.PasswordsDoNotMatch,
                RegistrationFailure.PrivacyPolicyNotAccepted => RegistrationError.PrivacyPolicyNotAccepted,
                _ => RegistrationError.UnknownError
            };

            return string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? Core_RegistrationResult.Failure(error)
                : Core_RegistrationResult.Failure(error, result.ErrorMessage);
        }
    }
}
