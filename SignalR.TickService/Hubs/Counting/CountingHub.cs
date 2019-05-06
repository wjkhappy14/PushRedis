
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalR.Tick.Hubs.Counting
{
    public class CountingHub : Hub
    {
        public async Task Send(int n)
        {
            await Clients.All.send(n);
        }
    }
}