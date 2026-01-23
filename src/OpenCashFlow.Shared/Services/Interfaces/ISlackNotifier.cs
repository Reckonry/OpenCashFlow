namespace Shared.Services.Interfaces
{
    public interface ISlackNotifier
    {
        Task NotifyAsync(string message, string? channel = null);
    }
}
