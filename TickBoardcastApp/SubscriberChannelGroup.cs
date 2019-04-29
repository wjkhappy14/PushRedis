using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels.Groups;

namespace TickBoardcastApp
{
    public class SubscriberChannelGroup : DefaultChannelGroup, IChannelGroup
    {
        public SubscriberChannelGroup(string name, IEventExecutor executor) : base(name, executor)
        {

        }
    }
}
