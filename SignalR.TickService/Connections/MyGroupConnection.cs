using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;

namespace SignalR.Tick.Connections
{
    public abstract class MyGroupConnection : PersistentConnection
    {
        private readonly Action _onReconnected;
        public MyGroupConnection() : this(onReconnected: () => { })
        {

        }

        private MyGroupConnection(Action onReconnected)
        {

            _onReconnected = onReconnected;
        }
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            JObject operation = JObject.Parse(data);

            int type = operation.Value<int>("type");
            string group = operation.Value<string>("group");
            string groupConnectionId = operation.Value<string>("connectionId") ?? connectionId;

            if (type == 1)
            {
                return Groups.Add(groupConnectionId, group);
            }
            else if (type == 2)
            {
                return Groups.Remove(groupConnectionId, group);
            }
            else if (type == 3)
            {
                return Groups.Send(group, operation.Value<string>("message"));
            }

            return base.OnReceived(request, connectionId, data);
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
