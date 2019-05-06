

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.Tick.Hubs.Auth
{
    [Authorize(RequireOutgoing=false)]
    public class IncomingAuthHub : NoAuthHub
    {
    }
}