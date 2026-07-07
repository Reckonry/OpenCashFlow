using OpenCashFlow.Application.Payments.GetPayments;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Tests.Payments.GetPayments;

public sealed class GetPaymentsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NormalizesPagingAndSort()
    {
        var reader = new FakePaymentQueryReader();
        var useCase = new GetPaymentsUseCase(reader);
        var tenantId = Guid.NewGuid();

        await useCase.ExecuteAsync(new GetPaymentsQuery(new PaymentListQuery(
            tenantId,
            PaymentId: null,
            EntryType: null,
            PaymentMethodId: null,
            DocumentTypeId: null,
            UserId: null,
            FromDate: null,
            ToDate: null,
            MinAmount: null,
            MaxAmount: null,
            Description: null,
            IsDeleted: null,
            SortBy: "",
            Desc: true,
            Page: -10,
            PageSize: 999)));

        Assert.NotNull(reader.LastQuery);
        Assert.Equal(1, reader.LastQuery!.Page);
        Assert.Equal(200, reader.LastQuery.PageSize);
        Assert.Equal("DateIns", reader.LastQuery.SortBy);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTenant_Fails()
    {
        var useCase = new GetPaymentsUseCase(new FakePaymentQueryReader());

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(new GetPaymentsQuery(new PaymentListQuery(
            Guid.Empty,
            PaymentId: null,
            EntryType: null,
            PaymentMethodId: null,
            DocumentTypeId: null,
            UserId: null,
            FromDate: null,
            ToDate: null,
            MinAmount: null,
            MaxAmount: null,
            Description: null,
            IsDeleted: null,
            SortBy: null,
            Desc: true,
            Page: 1,
            PageSize: 25))));
    }

    private sealed class FakePaymentQueryReader : IPaymentQueryReader
    {
        public PaymentListQuery? LastQuery { get; private set; }

        public Task<IReadOnlyList<PaymentListItem>> GetPaymentsAsync(PaymentListQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult<IReadOnlyList<PaymentListItem>>([]);
        }

        public Task<PaymentDetailResult?> GetPaymentDetailAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentDetailResult?>(null);
        }
    }
}
