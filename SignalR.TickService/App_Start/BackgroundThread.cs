using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNet.SignalR;
using SignalR.Tick.Hubs.DemoHub;

namespace SignalR.Tick
{
    public static class BackgroundThread
    {
        public static void Start()
        {
            ThreadPool.QueueUserWorkItem(x=>
            {
                IPersistentConnectionContext context = GlobalHost.ConnectionManager.GetConnectionContext<StreamingConnection>();
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<DemoHub>();
                while (true)
                {
                    try
                    {
                        context.Connection.Broadcast(DateTime.Now.ToString());
                        hubContext.Clients.All.fromArbitraryCode(DateTime.Now.ToString());
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("SignalR error thrown in Streaming broadcast: {0}", ex);
                    }
                    Thread.Sleep(1000);
                }
            });
        }
    }
}