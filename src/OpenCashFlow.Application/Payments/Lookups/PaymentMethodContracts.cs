namespace OpenCashFlow.Application.Payments.Lookups;

public sealed record PaymentMethodListItem(
    Guid PaymentMethodId,
    Guid? TenantId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder,
    bool IsDeleted,
    Guid? IsDeletedBy,
    string? IsDeletedWhy,
    DateTime? DateDeleted,
    Guid? CreatedBy,
    DateTime DateIns,
    Guid? EditedBy,
    DateTime? DateEdit);

public sealed record PaymentMethodResult(
    Guid PaymentMethodId,
    Guid? TenantId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder,
    bool IsDeleted,
    Guid? IsDeletedBy,
    string? IsDeletedWhy,
    DateTime? DateDeleted,
    Guid? CreatedBy,
    DateTime DateIns,
    Guid? EditedBy,
    DateTime? DateEdit);

public sealed record PaymentMethodCreateCommand(
    Guid PaymentMethodId,
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder);

public sealed record PaymentMethodUpdateCommand(
    Guid PaymentMethodId,
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder);
