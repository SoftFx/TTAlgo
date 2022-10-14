using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class BarSeriesBuffer : FeedBufferBase<BarData>, IWritableFeedBuffer<BarData>
    {
        public string Symbol { get; }

        public Feed.Types.Timeframe Timeframe { get; }

        public Feed.Types.MarketSide Side { get; }

        UtcTicks IFeedBuffer.this[int index] => _data[index].OpenTime;


        public BarSeriesBuffer(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, IFeedControllerContext context)
            : base(context)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            Side = side;
        }


        public async Task LoadFeed(UtcTicks from, int count)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            var bars = await _context.FeedHistory.QueryBarsAsync(Symbol, Side, Timeframe, from, count);
            _data.AddRange(bars);
        }

        public async Task LoadFeed(UtcTicks from, UtcTicks to)
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            var bars = await _context.FeedHistory.QueryBarsAsync(Symbol, Side, Timeframe, from, to);
            _data.AddRange(bars);
        }

        public void ApplyUpdate(BarData update)
        {
            var lastIndex = _data.Count - 1;

            if (_data[lastIndex].OpenTime == update.OpenTime)
                _data[lastIndex] = update;
            else if (_data[lastIndex].OpenTime < update.OpenTime)
                _data.Add(update);
        }
    }
}
