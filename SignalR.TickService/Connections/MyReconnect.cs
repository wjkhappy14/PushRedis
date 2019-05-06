
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class MyReconnect : PersistentConnection
    {
        private readonly Action _onReconnected;

        public MyReconnect()
            : this(onReconnected: () => { })
        {
        }

        public MyReconnect(Action onReconnected)
        {
            _onReconnected = onReconnected;
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            return null;
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            _onReconnected();
            return base.OnReconnected(request, connectionId);
        }
    }
}
