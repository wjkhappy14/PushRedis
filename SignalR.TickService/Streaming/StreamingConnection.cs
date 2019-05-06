

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalR.Tick
{
    public class StreamingConnection : PersistentConnection
    {
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            return base.OnConnected(request, connectionId);
        }
        protected override IList<string> GetSignals(string userId, string connectionId)
        {
            return base.GetSignals(userId, connectionId);
        }
        protected override TraceSource Trace => base.Trace;
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return base.OnReceived(request, connectionId, data);
        }

    }
}