namespace OpenCashFlow.Application.Auth.Register;

public sealed record RegisterCommand(
    string CompanyName,
    string Email,
    string Password,
    string ConfirmPassword,
    bool AcceptPrivacyPolicy,
    string? FirstName,
    string? LastName);
