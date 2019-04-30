using Core;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace TickBoardcastApp
{

    public class TickBoardcastHandler : SimpleChannelInboundHandler<string>
    {
        ProducerConsumer procon = new ProducerConsumer();
        static readonly ConcurrentDictionary<string, SubscriberChannelGroup> ChannelGroups = new ConcurrentDictionary<string, SubscriberChannelGroup>();
        static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public static ISubscriber RedisSub { get; } = Redis.GetSubscriber();

        private readonly System.Timers.Timer timer = new System.Timers.Timer(1);

        public IList<string> Items = new List<string>() {
            "GC1906",
            "CN1906",
            "CL1906",
            "SI1906",
            "DAX1906",
             "time",
            "MHI1906"
        };
        private static IChannelGroup DefaultGroup { get; set; }
        public TickBoardcastHandler()
        {
            RedisSub.Subscribe("SGX:CN:1906", (channel, value) =>
            {
                long unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                string msg = $"{unixTimeMilliseconds}";
                Console.WriteLine(msg);
                PushToChannelGroups(msg);
            });
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            long unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string now = unixTimeMilliseconds.ToString();
            PushToChannelGroups(now);
            PushToAllChannels(now);
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
            long unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //string.Format($"Welcome to {ServerSettings.Port} secure chat server!\n", Dns.GetHostName()
            contex.WriteAndFlushAsync(unixTimeMilliseconds);
            g.Add(contex.Channel);
            timer.Start();
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

        public void PushToChannelGroups(string msg)
        {
            if (ChannelGroups.Count > 0)
            {
                foreach (SubscriberChannelGroup group in ChannelGroups.Values)
                {
                    if (group.Count > 0)
                    {
                        group.WriteAndFlushAsync(msg);
                    }
                }
            }
        }
        public static void PushToAllChannels(string msg)
        {
            DefaultGroup.WriteAndFlushAsync($"{msg}{Environment.NewLine}");
        }

        protected override void ChannelRead0(IChannelHandlerContext contex, string msg)
        {
            //send message to all but this one
            long unixTimeMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string broadcast = $"{contex.Channel.RemoteAddress}:{msg}";
            Console.WriteLine(broadcast);
            string reply = $"{contex.Channel.RemoteAddress}:{msg}{Environment.NewLine}";
            if (Items.Contains(msg))
            {
                ChannelGroups[msg].Add(contex.Channel);
            }
            // DefaultGroup.WriteAndFlushAsync(broadcast, new EveryOneBut(contex.Channel.Id));
            contex.WriteAndFlushAsync(reply);

            if (string.Equals("time", msg, StringComparison.OrdinalIgnoreCase))
            {
                contex.WriteAndFlushAsync(unixTimeMilliseconds);
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
