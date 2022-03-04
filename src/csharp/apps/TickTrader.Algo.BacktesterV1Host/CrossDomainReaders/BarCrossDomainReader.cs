using System;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Serializers;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.BacktesterV1Host
{
    public sealed class BarCrossDomainReader : CrossDomainReader<BarData>
    {
        public BarCrossDomainReader(string dataBaseFolder, CrossDomainReaderRequest request) : base(dataBaseFolder, request)
        { }

        public BarCrossDomainReader(string dataBaseFolder, string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, DateTime from, DateTime to) :
            base(dataBaseFolder, new CrossDomainReaderRequest(new FeedCacheKey(symbol, frame, priceType), from, to))
        {}

        protected override SeriesStorage<DateTime, BarData> GetSeriesStorage()
        {
            return _dataBase.GetSeries(
                new DateTimeKeySerializer(),
                new BarSerializer(_request.Key.TimeFrame),
                b => b.OpenTime.ToUtcDateTime(),
                _request.Key.FullInfo,
                false);
        }
    }
}
