namespace OpenCashFlow.Application.Payments.Queries;

public sealed record PaymentListItem(
    Guid PaymentId,
    Guid TenantId,
    double Amount,
    string EntryType,
    Guid PaymentMethodId,
    string PaymentMethodName,
    Guid DocumentTypeId,
    string DocumentTypeName,
    string? Description,
    Guid UserId,
    string EmployeeFullName,
    bool IsDeleted,
    Guid? IsDeletedBy,
    string? IsDeletedWhy,
    DateTime? DateDeleted,
    Guid? CreatedBy,
    DateTime DateIns,
    Guid? EditedBy,
    DateTime? DateEdit);
