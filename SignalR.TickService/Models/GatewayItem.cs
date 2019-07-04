using Newtonsoft.Json;
using System.Collections.Generic;

namespace SignalR.Tick.Models
{
    public class GatewayItem
    {
        public static List<GatewayItem> A = new List<GatewayItem>()
        {
            new GatewayItem() { Location="上海", Host="47.103.24.234", Id=113 } ,
            new GatewayItem() { Location="北京", Host="39.105.122.159", Id=115 } ,
            new GatewayItem() { Location="深圳", Host="119.23.104.135", Id=117 } ,
            new GatewayItem() { Location="香港", Host="47.52.96.81", Id=119 } ,
        };

        public static List<GatewayItem> B = new List<GatewayItem>()
        {
            new GatewayItem() { Location="上海", Host="101.132.235.32", Id=112 } ,
            new GatewayItem() { Location="北京", Host="123.56.176.160", Id=114 } ,
            new GatewayItem() { Location="深圳", Host="120.78.225.138", Id=116 } ,
            new GatewayItem() { Location="香港", Host="47.75.97.147", Id=118 } ,
        };


        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("updatetime")]
        public string Updatetime { get; set; } = System.DateTime.Now.ToString("yyyy-MM-dd");

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("trade")]
        public TreadeItem Trade { get; set; } = new TreadeItem();

        [JsonProperty("quote")]
        public QuoteItem Quote { get; set; } = new QuoteItem();


    }

    public class TreadeItem
    {
        [JsonProperty("http")]
        public string Http { get; set; } = "/";

        [JsonProperty("ws")]
        public string Ws { get; set; } = "/ws-trade/ws";

        [JsonProperty("tcp")]
        public string Tcp { get; set; } = "8511";


    }
    public class QuoteItem
    {
        [JsonProperty("http")]
        public string Http { get; set; } = "/quote";

        [JsonProperty("ws")]
        public string Ws { get; set; } = "/ws-quote/ws";

        [JsonProperty("tcp")]
        public Dictionary<string, string> Tcp { get; set; } = new Dictionary<string, string>() {
            { "android","7111"},
            { "ios","7113"}
        };

    }
}