using System;

namespace SignalR.Tick.Hubs.StockTicker
{
    [Serializable]
    public class ClientItem
    {
        public string ConnectionId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }

        public ClientItem()
        {
        }

        public ClientItem(string name, string hash)
        {
            Name = name;
            Hash = hash;
            Id = Guid.NewGuid().ToString("d");
        }
    }
}