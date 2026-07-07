namespace OpenCashFlow.Contracts.DTOs.Payments;

public class Payment_DailyPayments
{
    public Guid DailyPaymentsID { get; set; } = Guid.NewGuid();
    public Guid TenantID { get; set; }
    public DateTime CashDate { get; set; }
    public double Total { get; set; }
    public DateTime DateIns { get; set; } = DateTime.UtcNow;
}

