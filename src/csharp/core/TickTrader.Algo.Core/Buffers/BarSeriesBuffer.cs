using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class BarSeriesBuffer : ILoadableFeedBuffer
    {
        private readonly CircularList<BarData> _data = new CircularList<BarData>();
        private readonly IFeedLoadContext _context;


        public string Symbol { get; }

        public Feed.Types.Timeframe Timeframe { get; }

        public Feed.Types.MarketSide Side { get; }

        public UtcTicks this[int index] => _data[index].OpenTime;

        public int Count => _data.Count;

        public bool IsLoaded { get; private set; }


        public BarSeriesBuffer(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, IFeedLoadContext context)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            Side = side;
            _context = context;
        }


        public async Task LoadFeed(UtcTicks from, int count)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            InitSub();
            var bars = await _context.FeedHistory.QueryBarsAsync(Symbol, Side, Timeframe, from, count);
            _data.AddRange(bars);
        }

        public async Task LoadFeed(UtcTicks from, UtcTicks to)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            InitSub();
            var bars = await _context.FeedHistory.QueryBarsAsync(Symbol, Side, Timeframe, from, to);
            _data.AddRange(bars);
        }


        private void InitSub()
        {
            _context.BarSub.Modify(BarSubUpdate.Upsert(new BarSubEntry(Symbol, Side, Timeframe)));
        }
    }
}
