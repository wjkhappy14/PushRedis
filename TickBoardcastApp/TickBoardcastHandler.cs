using Core;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace TickBoardcastApp
{
    
    public class TickBoardcastHandler : SimpleChannelInboundHandler<string>
    {
        static readonly ConcurrentDictionary<string, SubscriberChannelGroup> ChannelGroups = new ConcurrentDictionary<string, SubscriberChannelGroup>();
        static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public static ISubscriber RedisSub { get; } = Redis.GetSubscriber();

        private readonly System.Threading.Timer Timer = new System.Threading.Timer((x) =>
        {
            System.Diagnostics.Debug.WriteLine(x);
        });
        public IList<string> Items = new List<string>() {
            "GC1906",
            "CN1906",
            "CL1906",
            "SI1906",
            "DAX1906",
            "MHI1906"
        };
        private static IChannelGroup DefaultGroup { get; set; }
        public TickBoardcastHandler()
        {
            RedisSub.Subscribe("SGX:CN:1906", (channel, value) =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                string msg = $"{channel.ToString()}:{value.ToString()} localtime:{time}\r\n";
                Console.WriteLine(msg);
                PushRun(msg);
            });
        }

        public void InitSubscriberChannelGroups(IEventExecutor executor)
        {

            if (ChannelGroups.Count == 0)
            {
                foreach (string item in Items)
                {
                    SubscriberChannelGroup subscriberChannelGroup = new SubscriberChannelGroup(item, executor);
                    ChannelGroups.TryAdd(item, subscriberChannelGroup);
                }
            }
        }


        public override void ChannelActive(IChannelHandlerContext contex)
        {
            IChannelGroup g = DefaultGroup;
            if (g == null)
            {
                lock (this)
                {
                    if (DefaultGroup == null)
                    {
                        g = DefaultGroup = new DefaultChannelGroup(contex.Executor);
                    }
                }
            }
            InitSubscriberChannelGroups(contex.Executor);
            contex.WriteAndFlushAsync(string.Format("Welcome to {0} secure chat server!\n", Dns.GetHostName()));
            g.Add(contex.Channel);

        }

        class EveryOneBut : IChannelMatcher
        {
            public EveryOneBut(IChannelId id)
            {
                this.Id = id;
            }

            public IChannelId Id { get; private set; }

            public bool Matches(IChannel channel) => channel.Id != Id;
        }

        public void PushRun(string msg)
        {
            if (ChannelGroups.Count > 0)
            {
                foreach (SubscriberChannelGroup group in ChannelGroups.Values)
                {
                    if (group.Count > 0)
                    {
                        group.WriteAndFlushAsync($"{group.Name}:time:{DateTime.Now.ToString("HH:mm:ss.fff")} msg:{msg}\r\n");
                    }
                }
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, string msg)
        {
            //send message to all but this one
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            string broadcast = string.Format("[{0}] {1}\n", contex.Channel.RemoteAddress, msg);
            string response = string.Format("[you] {0}\n", msg);
            if (Items.Contains(msg))
            {
                ChannelGroups[msg].Add(contex.Channel);
            }
            DefaultGroup.WriteAndFlushAsync(broadcast, new EveryOneBut(contex.Channel.Id));

            contex.WriteAndFlushAsync(response);

            Console.WriteLine(broadcast);

            if (string.Equals("bye", msg, StringComparison.OrdinalIgnoreCase))
            {
                contex.CloseAsync();
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx) => ctx.Flush();

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine("{0}", e.StackTrace);
            ctx.CloseAsync();
        }

        public override bool IsSharable => true;
    }

}
