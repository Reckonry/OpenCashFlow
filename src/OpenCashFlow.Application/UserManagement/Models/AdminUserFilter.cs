namespace OpenCashFlow.Application.UserManagement.Models;

public sealed record AdminUserFilter(
    string? Search,
    Guid? TenantId,
    string? Role,
    bool? IsActive,
    bool? IsLocked,
    bool? EmailConfirmed,
    bool? TwoFactorEnabled,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    DateTime? LastLoginFrom,
    DateTime? LastLoginTo,
    int Page,
    int PageSize,
    string? SortBy,
    bool SortDescending);

