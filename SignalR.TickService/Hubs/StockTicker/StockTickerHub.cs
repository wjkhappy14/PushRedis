using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;

namespace SignalR.Tick.Hubs.StockTicker
{
    [HubName("stockTicker")]
    public class StockTickerHub : Hub
    {
        private StockTicker StockTicker { get; }

        public StockTickerHub() :
            this(StockTicker.Instance)
        {

        }

        public StockTickerHub(StockTicker stockTicker)
        {
            StockTicker = stockTicker;
        }

        public IEnumerable<Stock> GetAllStocks()
        {
            return StockTicker.GetAllStocks();
        }

        public string GetMarketState()
        {
            return StockTicker.MarketState.ToString();
        }

        /// <summary>
        /// 开市
        /// </summary>
        public void OpenMarket()
        {
            StockTicker.OpenMarket();
        }

        /// <summary>
        /// 休市
        /// </summary>
        public void CloseMarket()
        {
            StockTicker.CloseMarket();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            StockTicker.Reset();
        }
    }

}