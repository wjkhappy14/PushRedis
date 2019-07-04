using Newtonsoft.Json;
using System.Collections.Generic;

namespace SignalR.Tick.Models
{
    public class GatewayItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("updatetime")]
        public string Updatetime { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("trade")]
        public TreadeItem Trade { get; set; }

        [JsonProperty("quote")]
        public QuoteItem Quote { get; set; }


    }

    public class TreadeItem
    {
        [JsonProperty("http")]
        public string Http { get; set; }

        [JsonProperty("ws")]
        public string Ws { get; set; }

        [JsonProperty("tcp")]
        public string Tcp { get; set; }


    }
    public class QuoteItem
    {
        [JsonProperty("http")]
        public string Http { get; set; }

        [JsonProperty("ws")]
        public string Ws { get; set; }

        [JsonProperty("tcp")]
        public Dictionary<string,string> Tcp { get; set; }

    }
}