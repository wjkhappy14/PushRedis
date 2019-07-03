using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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



    }
    public class QuoteItem
    {


    }


}