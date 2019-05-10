using Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
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

        public IEnumerable<ContractQuoteFull> GetAllStocks()
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
        /// 订阅
        /// </summary>
        /// <param name="item"></param>
        public void Subscribe(string item)
        {


        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="item"></param>
        public void UnSubscribe(string item)
        {

        }
        /// <summary>
        /// 当前时间
        /// </summary>
        /// <returns></returns>
        public string Now()
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return now;
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