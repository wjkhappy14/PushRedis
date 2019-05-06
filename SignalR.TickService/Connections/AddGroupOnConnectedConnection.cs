
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    class AddGroupOnConnectedConnection : PersistentConnection
    {
        protected override async Task OnConnected(IRequest request, string connectionId)
        {
            await Groups.Add(connectionId, "test");
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Groups.Add(connectionId, "test2");
        }

        protected override async Task OnReceived(IRequest request, string connectionId, string data)
        {
            await Groups.Send("test2", "hey");
        }
    }
}
