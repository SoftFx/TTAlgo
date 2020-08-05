using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BarRateUpdate : IRateInfo
    {
        private QuoteInfo _lastQuote;
        private int _quoteCount;
        private DateTime _openTime;
        private DateTime _closeTime;

        public BarRateUpdate(DateTime barStartTime, DateTime barEndTime, QuoteInfo quote)
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
            _lastQuote = new QuoteInfo(symbol, _closeTime, bidBar.Close, askBar.Close);
        }

        public void Append(QuoteInfo quote)
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

        DateTime IRateInfo.Time => _openTime;
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
