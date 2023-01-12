using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class BarRateUpdate : IRateInfo
    {
        private QuoteInfo _lastQuote;
        private int _quoteCount;


        public UtcTicks OpenTime { get; }

        public UtcTicks CloseTime { get; }

        public bool IsPartialUpdate { get; private set; }


        public BarRateUpdate(BarData bidBar, BarData askBar, string symbol)
        {
            IsPartialUpdate = true;

            var bar = bidBar ?? askBar;
            OpenTime = bar.OpenTime;
            CloseTime = bar.CloseTime;
            BidBar = bidBar;
            AskBar = askBar;
            _quoteCount = 1;
            Symbol = symbol;
            _lastQuote = new QuoteInfo(symbol, CloseTime.AddMs(-1), bidBar?.Close, askBar?.Close);
        }

        public BarRateUpdate(BarUpdate update)
        {
            IsPartialUpdate = false;

            var bar = update.BidData ?? update.AskData;
            OpenTime = bar.OpenTime;
            CloseTime = bar.CloseTime;
            BidBar = update.BidData;
            AskBar = update.AskData;
            Symbol = update.Symbol;

            _quoteCount = 1;
            _lastQuote = new QuoteInfo(Symbol, CloseTime.AddMs(-1), BidBar?.Close, AskBar?.Close);
        }


        public void Replace(BarUpdate update)
        {
            IsPartialUpdate = false;

            BidBar = update.BidData;
            AskBar = update.AskData;

            _lastQuote = new QuoteInfo(Symbol, CloseTime.AddMs(-1), BidBar?.Close, AskBar?.Close);
            _quoteCount++;
        }

        public bool HasAsk => AskBar != null;
        public bool HasBid => BidBar != null;
        public string Symbol { get; }
        public BarData BidBar { get; private set; }
        public BarData AskBar { get; private set; }
        public QuoteInfo LastQuote => _lastQuote;

        UtcTicks IRateInfo.Time => OpenTime;
        DateTime IRateInfo.TimeUtc => OpenTime.ToUtcDateTime();
        double IRateInfo.Ask => AskBar.Close;
        double IRateInfo.AskHigh => AskBar.High;
        double IRateInfo.AskLow => AskBar.Low;
        double IRateInfo.AskOpen => AskBar.Open;
        double IRateInfo.Bid => BidBar.Close;
        double IRateInfo.BidHigh => BidBar.High;
        double IRateInfo.BidLow => BidBar.Low;
        double IRateInfo.BidOpen => BidBar.Open;
        int IRateInfo.NumberOfQuotes => _quoteCount;
        QuoteInfo IRateInfo.LastQuote => _lastQuote;
    }
}
