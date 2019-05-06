
using System;
using Microsoft.AspNet.SignalR;

namespace SignalR.Tick.Hubs.Auth
{
    public class InvokeAuthHub : Hub
    {
        [Authorize(Roles="Admin,Invoker")]
        public void InvokedFromClient()
        {
            Clients.All.invoked(Context.ConnectionId, DateTime.Now.ToString());
        }
    }
}