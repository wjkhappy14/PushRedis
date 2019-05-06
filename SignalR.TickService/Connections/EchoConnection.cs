
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class EchoConnection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Send(connectionId, data);
        }
    }
}
