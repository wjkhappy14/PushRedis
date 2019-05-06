
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalR.Tick.Hubs.Auth
{
    [AuthorizeClaims]
    public class HeaderAuthHub : Hub
    {
        public override Task OnConnected()
        {
            return Clients.Caller.display("Authenticated and Conencted!");
        }
    }

}