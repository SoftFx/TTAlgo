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


        public BarRateUpdate(UtcTicks barStartTime, UtcTicks barEndTime, QuoteInfo quote)
        {
            OpenTime = barStartTime;
            CloseTime = barEndTime;

            if (quote.HasBid)
                BidBar = new BarData(barStartTime, barEndTime, quote.Bid, 1);
            if (quote.HasAsk)
                AskBar = new BarData(barStartTime, barEndTime, quote.Ask, 1);

            _lastQuote = quote;
            Symbol = quote.Symbol;
            _quoteCount = 1;
        }

        public BarRateUpdate(BarData bidBar, BarData askBar, string symbol)
        {
            OpenTime = bidBar.OpenTime;
            CloseTime = bidBar.CloseTime;
            BidBar = bidBar;
            AskBar = askBar;
            _quoteCount = 1;
            Symbol = symbol;
            _lastQuote = new QuoteInfo(symbol, CloseTime.AddMs(-1), bidBar.Close, askBar.Close);
        }

        public BarRateUpdate(BarRateUpdate barUpdate)
        {
            Symbol = barUpdate.Symbol;
            BidBar = barUpdate.BidBar;
            AskBar = barUpdate.AskBar;
            _lastQuote = barUpdate._lastQuote;
            _quoteCount = barUpdate._quoteCount;
            OpenTime = barUpdate.OpenTime;
            CloseTime = barUpdate.CloseTime;
        }

        public void Append(QuoteInfo quote)
        {
            if (quote.HasBid)
            {
                if (HasBid)
                    BidBar.Append(quote.Bid, 1);
                else
                    BidBar = new BarData(OpenTime, CloseTime, quote.Bid, 1);
            }

            if (quote.HasAsk)
            {
                if (HasAsk)
                    AskBar.Append(quote.Ask, 1);
                else
                    AskBar = new BarData(OpenTime, CloseTime, quote.Ask, 1);
            }

            _lastQuote = quote;
            _quoteCount++;
        }

        public void Replace(BarUpdate update)
        {
            BidBar = update.BidData;
            AskBar = update.AskData;

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
