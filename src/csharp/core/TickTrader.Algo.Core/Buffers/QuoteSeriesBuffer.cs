using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesBuffer : ILoadableFeedBuffer
    {
        private readonly CircularList<QuoteInfo> _data = new CircularList<QuoteInfo>();
        private readonly IFeedLoadContext _context;


        public string Symbol { get; }

        public bool Level2 { get; }

        public UtcTicks this[int index] => _data[index].Time;

        public int Count => _data.Count;

        public bool IsLoaded { get; private set; }


        public QuoteSeriesBuffer(string symbol, bool level2, IFeedLoadContext context)
        {
            Symbol = symbol;
            Level2 = level2;
            _context = context;
        }


        public async Task LoadFeed(UtcTicks from, int count)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            InitSub();
            var bars = await _context.FeedHistory.QueryQuotesAsync(Symbol, from, count, Level2);
            _data.AddRange(bars);
        }

        public async Task LoadFeed(UtcTicks from, UtcTicks to)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            InitSub();
            var bars = await _context.FeedHistory.QueryQuotesAsync(Symbol, from, to, Level2);
            _data.AddRange(bars);
        }


        private void InitSub()
        {
            _context.QuoteSub.Modify(QuoteSubUpdate.Upsert(Symbol, 1));
        }
    }
}