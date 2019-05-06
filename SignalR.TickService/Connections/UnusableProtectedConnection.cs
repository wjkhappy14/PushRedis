
using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Connections
{
    public class UnusableProtectedConnection : PersistentConnection
    {
        protected override bool AuthorizeRequest(IRequest request)
        {
            return false;
        }
    }
}
