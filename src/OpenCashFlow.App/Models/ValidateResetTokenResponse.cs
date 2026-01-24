namespace OpenCashFlow.App.Models
{
    public class ValidateResetTokenResponse
    {
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public string? UserId { get; set; }
    }
}