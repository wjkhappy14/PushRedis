using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class ContractQuoteFull
    {
        private static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public ContractQuoteFull()
        {

        }

        public static List<Tuple<string, string>> Items = new List<Tuple<string, string>>() {
            Tuple.Create("AD", "AD1906"),
            Tuple.Create("BP", "BP1906"),
            Tuple.Create("CD", "CD1906"),
            Tuple.Create("CL", "CL1906"),
            Tuple.Create("CN", "CN1905"),
            Tuple.Create("HG", "HG1907"),
            Tuple.Create("GC", "GC1906"),
            Tuple.Create("EC", "EC1906"),
            Tuple.Create("HSI", "HSI1905"),
            Tuple.Create("MHI", "MHI1905"),
            Tuple.Create("DAX", "DAX1906"),
            Tuple.Create("NQ", "NQ1906"),
            Tuple.Create("SI", "SI1907")
        };

        public static Dictionary<string, string> SymbolItems = new Dictionary<string, string>() {
           {"AD1906","AD" },
           {"BP1906","BP" },
           {"CD1906","CD" },
           {"CL1906","CL" },
           {"CN1905","CN" },
           {"DAX1906","DAX" },
           {"EC1906","EC" },
           {"GC1906","GC" },
           {"HG1907","HG" },
           {"HSI1905","HSI" },
           {"MHI1905","MHI" },
           {"NQ1906","NQ" },
           {"SI1907", "SI" }

        };
        public static ContractQuoteFull Default(Tuple<string, string> item)
        {
            Tuple<string, string> rel = Items.First(x => x == item);
            string value = SymbolItems[rel.Item2];
            ContractQuoteFull contract = new ContractQuoteFull()
            {
                CommodityNo = value,
                BidPrice = "12564"
            };
            return contract;
        }

        public static Dictionary<string, string> Fake(string contractNo)
        {
            IDatabase db = Redis.GetDatabase(4);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            HashEntry[] values = db.HashGetAll(contractNo);
            foreach (HashEntry item in values)
            {
                if (item.Name == "Descriptor" || item.Name == "Parser")
                {
                }
                else
                {
                    dic.Add(item.Name, item.Value);
                }
            }
            return dic;
        }

        public long Time => DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public string CommodityNo { get; set; }

        public string ContractNo { get; set; }
        public string BidPrice { get; set; }

        public string BidPrice2 { get; set; }

        public string BidPrice3 { get; set; }
        public string BidPrice4 { get; set; }

        public string BidPrice5 { get; set; }
        public string BidPrice6
        { get; set; }

        public string BidPrice7
        { get; set; }

        public string BidPrice8
        { get; set; }

        public string BidPrice9
        { get; set; }

        public string BidPrice10
        { get; set; }

        public long BidSize
        { get; set; }

        public long BidSize2
        { get; set; }

        public long BidSize3
        { get; set; }

        public long BidSize4
        { get; set; }

        public long BidSize5
        { get; set; }

        public long BidSize6
        { get; set; }
        public long BidSize7
        { get; set; }

        public long BidSize8
        { get; set; }

        public long BidSize9
        { get; set; }

        public long BidSize10
        { get; set; }

        public string AskPrice
        { get; set; }

        public string AskPrice2
        { get; set; }

        public string AskPrice3
        { get; set; }

        public string AskPrice4
        { get; set; }
        public string AskPrice5
        { get; set; }

        public string AskPrice6
        { get; set; }

        public string AskPrice7
        { get; set; }
        public string AskPrice8
        { get; set; }

        public string AskPrice9
        { get; set; }

        public string AskPrice10
        { get; set; }

        public long AskSize
        { get; set; }
        public long AskSize2
        { get; set; }

        public long AskSize3
        { get; set; }

        public long AskSize4
        { get; set; }

        public long AskSize5
        { get; set; }

        public long AskSize6
        { get; set; }
        public long AskSize7
        { get; set; }

        public long AskSize8
        { get; set; }

        public long AskSize9
        { get; set; }
        public long AskSize10
        { get; set; }
        public string LastPrice
        { get; set; }

        public long LastSize
        { get; set; }

        public string OpenPrice
        { get; set; }

        public string HighPrice
        { get; set; }

        public string LowPrice
        { get; set; }
        public string NowClosePrice
        { get; set; }

        public string ClosePrice
        { get; set; }
        public long Volume
        { get; set; }
        public long TotalVolume
        { get; set; }

        public string PreSettlePrice
        { get; set; }
        public long TotalQty
        { get; set; }

        public long PositionQty
        { get; set; }

        public long PrePositionQty
        { get; set; }

        public long CurrentTime
        { get; set; }

        public override string ToString()
        {
            return "";
        }
    }
}
