using System;
using System.Collections.Generic;
using static SignalR.Tick.Hubs.StockTicker.StockTickerHub;

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