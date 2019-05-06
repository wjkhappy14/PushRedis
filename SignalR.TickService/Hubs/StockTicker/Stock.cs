﻿using Core;
using System;

namespace SignalR.Tick.Hubs.StockTicker
{
    public class Stock: ContractQuoteFull
    {
        private decimal _price;

        public string Symbol { get; set; }

        public decimal DayOpen { get; private set; }

        public decimal DayLow { get; private set; }

        public decimal DayHigh { get; private set; }

        public decimal LastChange { get; private set; }

        public decimal Change => Price - DayOpen;
        public long TickTime => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public double PercentChange => (double)Math.Round(Change / Price, 4);

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price == value)
                {
                    return;
                }

                LastChange = value - _price;
                _price = value;

                if (DayOpen == 0)
                {
                    DayOpen = _price;
                }
                if (_price < DayLow || DayLow == 0)
                {
                    DayLow = _price;
                }
                if (_price > DayHigh)
                {
                    DayHigh = _price;
                }
            }
        }
    }

}