
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalR.Tick.Hubs.ConnectDisconnect
{
    [HubName("StatusHub")]
    public class Status : Hub
    {
        public override Task OnDisconnected(bool stopCalled)
        {
            return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnConnected()
        {
            return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnReconnected()
        {
            return Clients.All.rejoined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public void Ping()
        {
            Clients.Caller.pong();
        }
    }
}
