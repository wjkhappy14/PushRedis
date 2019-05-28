using SignalR.Tick.Models;
using System;

namespace SignalR.Tick.Hubs.StockTicker
{
    [Serializable]
    public class Command
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public CommandType CmdType { get; set; }
        public Command()
        {

        }
    }
}