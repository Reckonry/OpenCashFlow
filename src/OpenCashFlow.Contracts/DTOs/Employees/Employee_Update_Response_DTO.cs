namespace OpenCashFlow.Contracts.DTOs.Employees
{
    public class Employee_Update_Response_DTO
    {
        public bool Success { get; set; }
        public bool EmailChanged { get; set; }
        public bool PinSentSuccessfully { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NewEmail { get; set; }
    }
}