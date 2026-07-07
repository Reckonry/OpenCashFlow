namespace OpenCashFlow.Application.Setup.GetSetupStatus;

public sealed record SetupStatusResult(
    bool RequiresSetup,
    bool HasCompanies,
    bool HasAdminUsers);
