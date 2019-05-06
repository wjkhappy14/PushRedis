
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class SyncErrorConnection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            throw new InvalidOperationException("This is a bug!");
        }
    }
}
