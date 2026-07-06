namespace OpenCashFlow.Application.Payments.Lookups;

public sealed record DocumentTypeListItem(
    Guid DocumentTypeId,
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

public sealed record DocumentTypeResult(
    Guid DocumentTypeId,
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

public sealed record DocumentTypeCreateCommand(
    Guid DocumentTypeId,
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder);

public sealed record DocumentTypeUpdateCommand(
    Guid DocumentTypeId,
    Guid TenantId,
    Guid UserId,
    string Name,
    string? Description,
    string? Icon,
    bool Visible,
    int DisplayOrder);
