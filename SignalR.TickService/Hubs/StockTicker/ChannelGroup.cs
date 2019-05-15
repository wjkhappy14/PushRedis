using System;
using System.Collections.Generic;

namespace SignalR.Tick.Hubs.StockTicker
{
    public class ChannelGroup
    {
        public List<Command> Messages { get; set; }
        public HashSet<string> Users { get; set; }

        public ChannelGroup()
        {
            Messages = new List<Command>();
            Users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}