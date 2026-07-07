using OpenCashFlow.Infrastructure.Persistence.Entities.Payments;
using Xunit;

namespace OpenCashFlow.Test.Tests.Unit
{
    public class CashExtensions_Tests
    {
        [Fact]
        public void IsCashLike_Cash_ReturnsTrue()
        {
            Assert.True(PaymentMethod.Cash.IsCashLike());
        }

        [Fact]
        public void IsCashLike_Pos_ReturnsFalse()
        {
            Assert.False(PaymentMethod.Pos.IsCashLike());
        }
    }
}

