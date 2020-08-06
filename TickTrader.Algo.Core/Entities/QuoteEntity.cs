using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class QuoteEntity : Quote
    {
        public static readonly BookEntry[] EmptyBook = new BookEntry[0];

        private readonly QuoteInfo _quote;


        public QuoteEntity(QuoteInfo quote)
        {
            _quote = quote;

            AskBook = quote.HasAsk ? Convert(quote.Asks) : EmptyBook;
            BidBook = quote.HasBid ? Convert(quote.Bids) : EmptyBook;
        }

        private static BookEntry[] Convert(ReadOnlySpan<QuoteBand> bands)
        {
            var bandList = new BookEntry[bands.Length];
            for (var i = 0; i < bands.Length; i++)
            {
                bandList[i] = new BookEntry(bands[i].Price, bands[i].Amount);
            }
            return bandList;
        }

        public string Symbol => _quote.Symbol;
        public DateTime Time => _quote.Time;
        public bool HasAsk => _quote.HasAsk;
        public bool HasBid => _quote.HasBid;
        public double Ask => _quote.Ask;
        public double Bid => _quote.Bid;
        public bool IsAskIndicative => _quote.IsAskIndicative;
        public bool IsBidIndicative => _quote.IsBidIndicative;

        public BookEntry[] BidBook { get; private set; }
        public BookEntry[] AskBook { get; private set; }

        public override string ToString()
        {
            var bookDepth = System.Math.Max(BidBook?.Length ?? 0, AskBook?.Length ?? 0);
            return $"{{{Bid}{(IsBidIndicative ? "i" : "")}/{Ask}{(IsAskIndicative ? "i" : "")} {Time} d{bookDepth}}}";
        }
    }
}
