using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalR.Tick.Models
{
    public class ElementTag
    {

        private ElementTag()
        {

        }
        public static ElementTag Create()
        {
            ElementTag tag = new ElementTag();


            return tag;
        }
        public string Tag { get; set; }
        public Tuple<string, string> Items { get; set; }

    }
}