using OpenCashFlow.Application.Setup.Ports;

namespace OpenCashFlow.Application.Setup.CompleteSetup;

public sealed class CompleteSetupUseCase(ISetupReader setupReader, ISetupWriter setupWriter) : ICompleteSetupUseCase
{
    public async Task<CompleteSetupResult> ExecuteAsync(CompleteSetupCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CompanyName)
            || string.IsNullOrWhiteSpace(command.AdminEmail)
            || string.IsNullOrWhiteSpace(command.AdminFirstName)
            || string.IsNullOrWhiteSpace(command.Language)
            || string.IsNullOrWhiteSpace(command.Currency)
            || string.IsNullOrWhiteSpace(command.Timezone)
            || string.IsNullOrWhiteSpace(command.Country))
        {
            return CompleteSetupResult.Fail(CompleteSetupFailure.InvalidInput, "Setup request contains invalid required fields.");
        }

        var currentStatus = await setupReader.GetStatusAsync(cancellationToken);
        if (!currentStatus.RequiresSetup)
        {
            return CompleteSetupResult.Fail(CompleteSetupFailure.AlreadyConfigured, "This OpenCashFlow instance is already configured.");
        }

        if (currentStatus.HasCompanies || currentStatus.HasAdminUsers)
        {
            return CompleteSetupResult.Fail(CompleteSetupFailure.PartiallyConfigured, "Setup cannot continue because this instance is partially configured.");
        }

        if (!IsStrongPassword(command.AdminPassword))
        {
            return CompleteSetupResult.Fail(
                CompleteSetupFailure.WeakPassword,
                "Admin password must be at least 8 characters and include upper, lower, digit, and special characters.");
        }

        var completedStatus = await setupWriter.CompleteAsync(command, cancellationToken);
        return CompleteSetupResult.Ok(completedStatus);
    }

    private static bool IsStrongPassword(string password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Length >= 8
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
