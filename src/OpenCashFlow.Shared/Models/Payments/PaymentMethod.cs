namespace Shared.Models.Payments
{
    public enum PaymentMethod
    {
        Cash = 1,
        Pos = 2,
        BankTransfer = 3,
        Other = 99
    }

    public static class PaymentMethodExtensions
    {
        public static bool IsCashLike(this PaymentMethod m) => m == PaymentMethod.Cash;
    }
}

