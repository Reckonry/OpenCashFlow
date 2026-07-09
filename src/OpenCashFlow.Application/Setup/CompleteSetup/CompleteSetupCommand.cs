namespace OpenCashFlow.Application.Setup.CompleteSetup;

public sealed record CompleteSetupCommand(
    string CompanyName,
    string AdminEmail,
    string AdminFirstName,
    string? AdminLastName,
    string Language,
    string Currency,
    string Timezone,
    string Country);
