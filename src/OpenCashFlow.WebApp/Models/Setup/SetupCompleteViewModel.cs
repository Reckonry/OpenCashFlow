namespace OpenCashFlow.WebApp.Models.Setup;

public sealed class SetupCompleteViewModel
{
    public required string AdminEmail { get; init; }
    public required string TemporaryAdminPassword { get; init; }
}
