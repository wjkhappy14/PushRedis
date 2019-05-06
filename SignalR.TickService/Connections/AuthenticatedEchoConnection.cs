
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalR.Tick.Connections
{
    public class AuthenticatedEchoConnection : PersistentConnection
    {
        protected override bool AuthorizeRequest(IRequest request)
        {
            return request.User != null && request.User.Identity.IsAuthenticated;
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }
    }
}
