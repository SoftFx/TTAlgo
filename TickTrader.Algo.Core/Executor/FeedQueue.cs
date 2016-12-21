using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Math;

namespace TickTrader.Algo.Core
{
    internal class FeedQueue
    {
        private Queue<QuoteAggregation> innerQueue = new Queue<QuoteAggregation>();
        private int maxSavedQuotes;

        public FeedQueue(int maxSavedQuotes = 20)
        {
            this.maxSavedQuotes = maxSavedQuotes;
        }

        public int Count { get { return innerQueue.Count; } }

        public void Enqueue(Quote quote)
        {
            var aggregation = innerQueue.FirstOrDefault(a => a.Symbol == quote.Symbol);
            if (aggregation != null)
                aggregation.Add(quote, maxSavedQuotes);
            else
                innerQueue.Enqueue(new QuoteAggregation(quote));
        }

        public RateUpdate Dequeue()
        {
            var aggregation = innerQueue.Dequeue();
            aggregation.Close();
            return aggregation;
        }

        public void Clear()
        {
            innerQueue.Clear();
        }
            
        private class QuoteAggregation : RateUpdate
        {
            private Queue<Quote> quotes = new Queue<Quote>();

            public QuoteAggregation(Quote firstQuote)
            {
                Bid = firstQuote.Bid;
                Ask = firstQuote.Ask;
                BidHigh = firstQuote.Bid;
                BidLow = firstQuote.Bid;
                AskHigh = firstQuote.Ask;
                AskLow = firstQuote.Ask;
                Time = firstQuote.Time;
                Symbol = firstQuote.Symbol;
                NumberOfQuotes = 1;
                quotes.Enqueue(firstQuote);
            }

            public string Symbol { get; private set; }
            public DateTime Time { get; private set; }
            public double Ask { get; private set; }
            public double AskHigh { get; private set; }
            public double AskLow { get; private set; }
            public double Bid { get; private set; }
            public double BidHigh { get; private set; }
            public double BidLow { get; private set; }
            public double NumberOfQuotes { get; private set; }
            public Quote[] LastQuotes { get; private set; }

            public void Add(Quote quote, int queueLimit)
            {
                if (quotes.Count > queueLimit)
                    quotes.Dequeue();
                quotes.Enqueue(quote);

                NumberOfQuotes++;

                Ask = quote.Ask;
                Bid = quote.Bid;
                if (AskHigh < quote.Ask)
                    AskHigh = quote.Ask;
                if (AskLow > quote.Ask)
                    AskLow = quote.Ask;
                if (BidHigh < quote.Bid)
                    BidHigh = quote.Bid;
                if (BidLow > quote.Bid)
                    BidLow = quote.Bid;
            }

            public void Close()
            {
                LastQuotes = quotes.ToArray();
                Array.Reverse(LastQuotes);
                quotes = null;
            }
        }
    }
}
