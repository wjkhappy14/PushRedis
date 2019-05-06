using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Hubs.Auth
{
    [Authorize]
    public class AuthHub : NoAuthHub
    {
    }
}