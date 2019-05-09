using Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SignalR.Tick.Hubs.StockTicker
{
    public class StockTicker
    {

        private static ConnectionMultiplexer Redis = RedisHelper.RedisMultiplexer();
        public static ISubscriber RedisSub { get; } = Redis.GetSubscriber();
        // Singleton instance
        private readonly static Lazy<StockTicker> _instance = new Lazy<StockTicker>(() => new StockTicker(GlobalHost.ConnectionManager.GetHubContext<StockTickerHub>().Clients));

        private readonly object _marketStateLock = new object();
        private readonly object _updateStockPricesLock = new object();

        private readonly ConcurrentDictionary<string, ContractQuoteFull> _stocks = new ConcurrentDictionary<string, ContractQuoteFull>();

        // Stock can go up or down by a percentage of this factor on each change
        private readonly double _rangePercent = 0.002;

        private readonly Random _updateOrNotRandom = new Random();

        private volatile bool _updatingStockPrices;
        private volatile MarketState _marketState;

        private StockTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            LoadDefaultStocks();
            ContractQuoteFull.Items.ForEach(item =>
            {
                RedisSub.Subscribe(item, (channel, value) =>
                {
                    //string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    //string msg = $"{now}/{value}";


                    Clients.All.updateStockPrice(value);
                });
            });
        }

        public static StockTicker Instance => _instance.Value;

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public MarketState MarketState
        {
            get => _marketState;
            private set => _marketState = value;
        }

        public IEnumerable<ContractQuoteFull> GetAllStocks() => _stocks.Values;

        public void OpenMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketState.Open)
                {
                    Clients.All.updateStockPrice(2);

                    MarketState = MarketState.Open;

                    BroadcastMarketStateChange(MarketState.Open);
                }
            }
        }

        public void CloseMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState == MarketState.Open)
                {
                    MarketState = MarketState.Closed;

                    BroadcastMarketStateChange(MarketState.Closed);
                }
            }
        }

        public void Reset()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketState.Closed)
                {
                    throw new InvalidOperationException("Market must be closed before it can be reset.");
                }

                LoadDefaultStocks();
                BroadcastMarketReset();
            }
        }

        private void LoadDefaultStocks()
        {
            _stocks.Clear();
            ContractQuoteFull.Items.ForEach(stock => _stocks.TryAdd(stock, ContractQuoteFull.Default(stock)));
        }

     
        private void BroadcastMarketStateChange(MarketState marketState)
        {
            switch (marketState)
            {
                case MarketState.Open:
                    Clients.All.marketOpened();
                    break;
                case MarketState.Closed:
                    Clients.All.marketClosed();
                    break;
                default:
                    break;
            }
        }

        private void BroadcastMarketReset()
        {
            Clients.All.marketReset();
        }
    }

    public enum MarketState
    {
        Closed,
        Open
    }
}