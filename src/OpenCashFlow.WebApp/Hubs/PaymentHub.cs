using Microsoft.AspNetCore.SignalR;

namespace OpenCashFlow.WebApp.Hubs
{
    public class PaymentHub : Hub
    {
        // In this hub we do not need to expose client-callable methods,
        // because on the server side we only send notifications to clients.
    }
}
