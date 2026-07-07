namespace OpenCashFlow.Application.Abstractions
{
    public interface ISlackNotifier
    {
        Task NotifyAsync(string message, string? channel = null);
    }
}
