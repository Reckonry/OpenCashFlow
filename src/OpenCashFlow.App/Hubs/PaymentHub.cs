using Microsoft.AspNetCore.SignalR;

namespace OpenCashFlow.App.Hubs
{
    public class PaymentHub : Hub
    {
        // In this hub we do not need to expose client-callable methods,
        // because on the server side we only send notifications to clients.
    }
}
