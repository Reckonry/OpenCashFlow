namespace OpenCashFlow.Application.Abstractions;

public sealed record PaymentDraft(
    Guid PaymentId,
    Guid TenantId,
    Guid RequestId,
    decimal Amount,
    string EntryType,
    Guid PaymentMethodId,
    Guid DocumentTypeId,
    Guid UserId,
    DateTime DateIns,
    string? Description);

public sealed record PaymentUpdateDraft(
    Guid PaymentId,
    Guid TenantId,
    decimal Amount,
    string EntryType,
    Guid PaymentMethodId,
    Guid DocumentTypeId,
    Guid UserId,
    DateTime DateIns,
    string? Description);

public sealed record PaymentSnapshot(
    Guid PaymentId,
    Guid TenantId,
    Guid RequestId,
    decimal Amount,
    string EntryType,
    Guid PaymentMethodId,
    Guid DocumentTypeId,
    Guid UserId,
    DateTime DateIns,
    string? Description);

public sealed record PaymentMethodSnapshot(
    Guid PaymentMethodId,
    string? Name);

public sealed record PaymentUpdateAudit(
    PaymentSnapshot Before,
    PaymentSnapshot After,
    Guid ActorUserId);

public sealed record PaymentDeletedAudit(
    PaymentSnapshot Payment,
    Guid ActorUserId);
