using System;
using System.Diagnostics;
using System.Threading;
using Core;
using Microsoft.AspNet.SignalR;
using SignalR.Tick.Hubs.DemoHub;
using SignalR.Tick.Models;
using StackExchange.Redis;

namespace SignalR.Tick
{
    public static class BackgroundThread
    {
        private static readonly ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        static readonly IDatabase DB = Redis.GetDatabase(2);
        public static void Start()
        {
            ReplyContent<string> reply = new ReplyContent<string>();
            reply.CmdType = CommandType.TimeNow;
            ThreadPool.QueueUserWorkItem(x =>
            {
                IPersistentConnectionContext context = GlobalHost.ConnectionManager.GetConnectionContext<RawConnection>();
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<DemoHub>();
                while (true)
                {
                    try
                    {
                        reply.Result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        DB.PublishAsync("now", reply.Result);
                        context.Connection.Broadcast(reply);
                        hubContext.Clients.All.fromArbitraryCode(reply);
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