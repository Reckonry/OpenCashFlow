namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminRoleSummary(
    string RoleName,
    string Description,
    int UserCount);

