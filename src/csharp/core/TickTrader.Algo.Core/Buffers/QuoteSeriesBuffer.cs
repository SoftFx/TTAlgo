using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesBuffer : FeedBufferBase<QuoteInfo>, IWritableFeedBuffer<QuoteInfo>, ITimeRef
    {
        public string Symbol { get; }

        public bool Level2 { get; }

        UtcTicks ITimeRef.this[int index] => _data[index].Time;

        public ITimeRef Timeline => this;


        public QuoteSeriesBuffer(string symbol, bool level2, IFeedControllerContext context)
            : base(context)
        {
            Symbol = symbol;
            Level2 = level2;
        }


        public async Task LoadFeed(UtcTicks from, int count)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            var bars = await _context.FeedHistory.QueryQuotesAsync(Symbol, from, count, Level2);
            _data.AddRange(bars);
        }

        public async Task LoadFeed(UtcTicks from, UtcTicks to)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            var bars = await _context.FeedHistory.QueryQuotesAsync(Symbol, from, to, Level2);
            _data.AddRange(bars);
        }

        public void ApplyUpdate(QuoteInfo update)
        {
            _data.Add(update);
        }
    }
}