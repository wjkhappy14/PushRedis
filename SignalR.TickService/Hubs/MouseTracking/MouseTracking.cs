
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Hubs.MouseTracking
{
    public class MouseTracking : Hub
    {
        private static long _id;

        public void Join()
        {
            Clients.Caller.id = Interlocked.Increment(ref _id);
        }

        public void Move(int x, int y)
        {
            Clients.Others.move(Clients.Caller.id, x, y);
        }
    }
}