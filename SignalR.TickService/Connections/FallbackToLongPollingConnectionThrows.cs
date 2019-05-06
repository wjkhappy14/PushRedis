
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class FallbackToLongPollingConnectionThrows : PersistentConnection
    {
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            throw new InvalidOperationException();
        }
    }
}
