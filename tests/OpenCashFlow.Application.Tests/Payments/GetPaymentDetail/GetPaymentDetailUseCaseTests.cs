using OpenCashFlow.Application.Payments.GetPaymentDetail;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;

namespace OpenCashFlow.Application.Tests.Payments.GetPaymentDetail;

public sealed class GetPaymentDetailUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenReaderReturnsNull_ReturnsNull()
    {
        var useCase = new GetPaymentDetailUseCase(new FakePaymentQueryReader());

        var result = await useCase.ExecuteAsync(new GetPaymentDetailQuery(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyPaymentId_Fails()
    {
        var useCase = new GetPaymentDetailUseCase(new FakePaymentQueryReader());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(new GetPaymentDetailQuery(Guid.Empty, Guid.NewGuid())));
    }

    private sealed class FakePaymentQueryReader : IPaymentQueryReader
    {
        public Task<IReadOnlyList<PaymentListItem>> GetPaymentsAsync(PaymentListQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<PaymentListItem>>([]);
        }

        public Task<PaymentDetailResult?> GetPaymentDetailAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentDetailResult?>(null);
        }
    }
}
