
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class AsyncOnConnectedConnection : PersistentConnection
    {
        protected override async Task OnConnected(IRequest request, string connectionId)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
