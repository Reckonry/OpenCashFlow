namespace OpenCashFlow.Application.Payments.Queries;

public sealed record PaymentListQuery(
    Guid TenantId,
    Guid? PaymentId,
    string? EntryType,
    Guid? PaymentMethodId,
    Guid? DocumentTypeId,
    Guid? UserId,
    DateTime? FromDate,
    DateTime? ToDate,
    double? MinAmount,
    double? MaxAmount,
    string? Description,
    bool? IsDeleted,
    string? SortBy,
    bool Desc,
    int Page,
    int PageSize)
{
    public PaymentListQuery Normalize()
    {
        return this with
        {
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "DateIns" : SortBy,
            Page = Math.Max(Page, 1),
            PageSize = Math.Clamp(PageSize, 1, 200)
        };
    }
}
