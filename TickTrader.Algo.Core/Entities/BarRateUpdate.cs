﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BarRateUpdate : RateUpdate
    {
        private QuoteEntity _lastQuote;
        private int _quoteCount;
        private DateTime _openTime;
        private DateTime _closeTime;

        public BarRateUpdate(DateTime barStartTime, DateTime barEndTime, QuoteEntity quote)
        {
            _openTime = barStartTime;
            _closeTime = barEndTime;

            if (quote.HasBid)
                BidBar = new BarEntity(barStartTime, barEndTime, quote.Bid, 1);
            if (quote.HasAsk)
                AskBar = new BarEntity(barStartTime, barEndTime, quote.Ask, 1);

            _lastQuote = quote;
            Symbol = quote.Symbol;
            _quoteCount = 1;
        }

        public BarRateUpdate(BarEntity bidBar, BarEntity askBar, string symbol)
        {
            _openTime = bidBar.OpenTime;
            _closeTime = bidBar.CloseTime;
            BidBar = bidBar;
            AskBar = askBar;
            _quoteCount = 1;
            Symbol = symbol;
            _lastQuote = new QuoteEntity(symbol, _closeTime, bidBar.Close, askBar.Close);
        }

        public void Append(QuoteEntity quote)
        {
            if (quote.HasBid)
            {
                if (HasBid)
                    BidBar.Append(quote.Bid, 1);
                else
                    BidBar = new BarEntity(_openTime, _closeTime, quote.Bid, 1);
            }

            if (quote.HasAsk)
            {
                if (HasAsk)
                    AskBar.Append(quote.Ask, 1);
                else
                    AskBar = new BarEntity(_openTime, _closeTime, quote.Ask, 1);
            }

            _lastQuote = quote;
            _quoteCount++;
        }

        public bool HasAsk => AskBar != null;
        public bool HasBid => BidBar != null;
        public string Symbol { get; }
        public BarEntity BidBar { get; private set; }
        public BarEntity AskBar { get; private set; }

        DateTime RateUpdate.Time => _openTime;
        double RateUpdate.Ask => AskBar.Close;
        double RateUpdate.AskHigh => AskBar.High;
        double RateUpdate.AskLow => AskBar.Low;
        double RateUpdate.AskOpen => AskBar.Open;
        double RateUpdate.Bid => BidBar.Close;
        double RateUpdate.BidHigh => BidBar.High;
        double RateUpdate.BidLow => BidBar.Low;
        double RateUpdate.BidOpen => BidBar.Open;
        int RateUpdate.NumberOfQuotes => _quoteCount;
        Quote RateUpdate.LastQuote => _lastQuote;
    }
}