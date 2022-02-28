using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class BarRateUpdate : IRateInfo
    {
        private QuoteInfo _lastQuote;
        private int _quoteCount;
        private long _openTime;
        private long _closeTime;

        public BarRateUpdate(long barStartTime, long barEndTime, QuoteInfo quote)
        {
            _openTime = barStartTime;
            _closeTime = barEndTime;

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
            _openTime = bidBar.OpenTime;
            _closeTime = bidBar.CloseTime;
            BidBar = bidBar;
            AskBar = askBar;
            _quoteCount = 1;
            Symbol = symbol;
            _lastQuote = new QuoteInfo(symbol, TimeTicks.FromMs(_closeTime - 1), bidBar.Close, askBar.Close);
        }

        public BarRateUpdate(BarRateUpdate barUpdate)
        {
            Symbol = barUpdate.Symbol;
            BidBar = barUpdate.BidBar;
            AskBar = barUpdate.AskBar;
            _lastQuote = barUpdate._lastQuote;
            _quoteCount = barUpdate._quoteCount;
            _openTime = barUpdate._openTime;
            _closeTime = barUpdate._closeTime;
        }

        public void Append(QuoteInfo quote)
        {
            if (quote.HasBid)
            {
                if (HasBid)
                    BidBar.Append(quote.Bid, 1);
                else
                    BidBar = new BarData(_openTime, _closeTime, quote.Bid, 1);
            }

            if (quote.HasAsk)
            {
                if (HasAsk)
                    AskBar.Append(quote.Ask, 1);
                else
                    AskBar = new BarData(_openTime, _closeTime, quote.Ask, 1);
            }

            _lastQuote = quote;
            _quoteCount++;
        }

        public void Append(BarRateUpdate barUpdate)
        {
            var quoteTime = _openTime;

            if (barUpdate.HasBid)
            {
                quoteTime = barUpdate.BidBar.CloseTime;
                if (HasBid)
                    BidBar.AppendPart(barUpdate.BidBar);
                else
                    BidBar = new BarData(barUpdate.BidBar);
            }

            if (barUpdate.HasAsk)
            {
                quoteTime = barUpdate.AskBar.CloseTime;
                if (HasAsk)
                    AskBar.AppendPart(barUpdate.AskBar);
                else
                    AskBar = new BarData(barUpdate.AskBar);
            }

            _lastQuote = new QuoteInfo(Symbol, TimeTicks.FromMs(quoteTime), barUpdate.BidBar?.Close, barUpdate.AskBar?.Close);
            _quoteCount++;
        }

        public bool HasAsk => AskBar != null;
        public bool HasBid => BidBar != null;
        public string Symbol { get; }
        public BarData BidBar { get; private set; }
        public BarData AskBar { get; private set; }
        public QuoteInfo LastQuote => _lastQuote;

        long IRateInfo.UtcTicks => TimeTicks.FromMs(_openTime);
        long IRateInfo.UtcMs => _openTime;
        DateTime IRateInfo.Time => TimeMs.ToUtc(_openTime);
        DateTime IRateInfo.TimeUtc => TimeMs.ToUtc(_openTime);
        Timestamp IRateInfo.Timestamp => TimeMs.ToTimestamp(_openTime);
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
