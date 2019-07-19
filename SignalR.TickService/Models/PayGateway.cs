using Newtonsoft.Json;
using System.Collections.Generic;

namespace SignalR.Tick.Models
{
    public class PayGateway
    {
        [JsonProperty("payurl")]
        public string Payurl { get; set; } = "https://applicate.wintonefutures.com/operate/#/charge";

        [JsonProperty("items")]
        public List<GatewayItem> Items { get; set; }// = GatewayItem.A;

    }
}