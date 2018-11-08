using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class BarRateUpdate : RateUpdate
    {
        private QuoteEntity _lastQuote;
        private int _quoteCount;

        public BarRateUpdate(DateTime barStartTime, DateTime barEndTime, QuoteEntity quote)
        {
            BidBar = new BarEntity(barStartTime, barEndTime, quote.Bid, 1);
            AskBar = new BarEntity(barStartTime, barEndTime, quote.Ask, 1);
            _lastQuote = quote;
            Symbol = quote.Symbol;
            _quoteCount = 1;
        }

        public BarRateUpdate(BarEntity bidBar, BarEntity askBar, string symbol)
        {
            BidBar = bidBar;
            AskBar = askBar;
            _quoteCount = 1;
            Symbol = symbol;
            _lastQuote = new QuoteEntity(symbol, bidBar.CloseTime, bidBar.Close, askBar.Close);
        }

        public void Append(QuoteEntity quote)
        {
            BidBar.Append(quote.Bid, 1);
            AskBar.Append(quote.Ask, 1);
            _lastQuote = quote;
            _quoteCount++;
        }

        public string Symbol { get; }
        public BarEntity BidBar { get; }
        public BarEntity AskBar { get; }

        DateTime RateUpdate.Time => AskBar.OpenTime;
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
