using Microsoft.AspNet.SignalR;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class PreserializedJsonConnection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            ArraySegment<byte> jsonBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
            return Connection.Send(connectionId, jsonBytes);
        }
    }
}
