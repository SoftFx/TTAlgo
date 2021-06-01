using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class TickSeriesReader : SeriesReader
    {
        private string _symbol;
        private IEnumerable<QuoteInfo> _src;
        private IEnumerator<QuoteInfo> _e;
        //private QuoteEntity _current;

        public TickSeriesReader(string symbol, IEnumerable<QuoteInfo> src)
        {
            _symbol = symbol;
            _src = src;
        }

        public TickSeriesReader(string symbol, ITickStorage storage)
        {
            _symbol = symbol;
            _src = storage.GetQuoteStream();
        }

        public override SeriesReader Clone()
        {
            return new TickSeriesReader(_symbol, _src);
        }

        public override bool MoveNext()
        {
            if (_e.MoveNext())
            {
                Current = _e.Current;
                //UpdateBars(quote);
                return true;
            }
            else
                return false;
        }

        public override void Start()
        {
            _e = _src.GetEnumerator();
        }

        public override void Stop()
        {
            _e.Dispose();
        }

        //private void UpdateBars(QuoteEntity quote)
        //{
        //    if (!double.IsNaN(quote.Bid))
        //    {
        //        foreach (var rec in _bidBars.Values)
        //            rec.AppendQuote(quote.CreatingTime, quote.Bid, 1);
        //    }

        //    if (!double.IsNaN(quote.Ask))
        //    {
        //        foreach (var rec in _askBars.Values)
        //            rec.AppendQuote(quote.CreatingTime, quote.Ask, 1);
        //    }
        //}
    }
}
