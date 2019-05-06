
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class FallbackToLongPollingConnection : PersistentConnection
    {
        protected override async Task OnConnected(IRequest request, string connectionId)
        {
            string transport = request.QueryString["transport"];

            if (transport != "longPolling")
            {
                await Task.Delay(7000);
            }

            await base.OnConnected(request, connectionId);
        }
    }
}
