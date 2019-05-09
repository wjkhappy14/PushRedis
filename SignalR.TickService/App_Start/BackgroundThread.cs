using System;
using System.Diagnostics;
using System.Threading;
using Core;
using Microsoft.AspNet.SignalR;
using SignalR.Tick.Hubs.DemoHub;
using StackExchange.Redis;

namespace SignalR.Tick
{
    public static class BackgroundThread
    {
        private static readonly ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        static readonly IDatabase DB = Redis.GetDatabase(2);
        public static void Start()
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                IPersistentConnectionContext context = GlobalHost.ConnectionManager.GetConnectionContext<StreamingConnection>();
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<DemoHub>();
                while (true)
                {
                    try
                    {
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DB.PublishAsync("now", now);
                        context.Connection.Broadcast(now);
                        hubContext.Clients.All.fromArbitraryCode(now);
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