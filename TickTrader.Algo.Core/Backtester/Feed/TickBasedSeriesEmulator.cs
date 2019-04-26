using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class TickBasedSeriesEmulator : FeedSeriesEmulator
    {
        //private string _symbol;
        private IEnumerable<QuoteEntity> _src;
        private IEnumerator<QuoteEntity> _e;
        //private QuoteEntity _current;

        public TickBasedSeriesEmulator(string symbol, IEnumerable<QuoteEntity> src)
        {
            _src = src;
        }

        public TickBasedSeriesEmulator(string symbol, ITickStorage storage)
        {
            _src = storage.GetQuoteStream();
        }

        public override bool MoveNext()
        {
            if (_e.MoveNext())
            {
                var quote = _e.Current;
                Current = quote;
                UpdateBars(quote);
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

        private void UpdateBars(QuoteEntity quote)
        {
            if (!double.IsNaN(quote.Bid))
            {
                foreach (var rec in _bidBars.Values)
                    rec.AppendQuote(quote.CreatingTime, quote.Bid, 1);
            }

            if (!double.IsNaN(quote.Ask))
            {
                foreach (var rec in _askBars.Values)
                    rec.AppendQuote(quote.CreatingTime, quote.Ask, 1);
            }
        }
    }
}
