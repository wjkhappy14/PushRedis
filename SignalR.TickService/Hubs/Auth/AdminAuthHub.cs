

using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Hubs.Auth
{
    [Authorize(Roles="Admin")]
    public class AdminAuthHub : NoAuthHub
    {
    }
}