using OpenCashFlow.Application.Setup.Ports;
using System.Security.Cryptography;

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

        var temporaryAdminPassword = GenerateTemporaryPassword();
        var completedStatus = await setupWriter.CompleteAsync(command, temporaryAdminPassword, cancellationToken);
        return CompleteSetupResult.Ok(completedStatus, temporaryAdminPassword);
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*()-_=+";
        const string all = upper + lower + digits + symbols;

        Span<char> password =
        [
            Pick(upper),
            Pick(lower),
            Pick(digits),
            Pick(symbols),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all),
            Pick(all)
        ];

        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    private static char Pick(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}
