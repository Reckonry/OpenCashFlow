using OpenCashFlow.Application.Setup.GetSetupStatus;

namespace OpenCashFlow.Application.Setup.CompleteSetup;

public enum CompleteSetupFailure
{
    None,
    AlreadyConfigured,
    PartiallyConfigured,
    WeakPassword,
    InvalidInput
}

public sealed record CompleteSetupResult(
    bool Success,
    CompleteSetupFailure Failure,
    string? Message,
    SetupStatusResult? Status,
    string? TemporaryAdminPassword)
{
    public static CompleteSetupResult Ok(SetupStatusResult status, string temporaryAdminPassword)
        => new(true, CompleteSetupFailure.None, null, status, temporaryAdminPassword);

    public static CompleteSetupResult Fail(CompleteSetupFailure failure, string message)
        => new(false, failure, message, null, null);
}
