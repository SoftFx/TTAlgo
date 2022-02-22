using System;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Serializers;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.BacktesterV1Host
{
    public sealed class TickCrossDomainReader : CrossDomainReader<QuoteInfo>
    {
        public TickCrossDomainReader(string dataBaseFolder, CrossDomainReaderRequest request) : base(dataBaseFolder, request)
        { }

        public TickCrossDomainReader(string dataBaseFolder, string symbol, Feed.Types.Timeframe frame, DateTime from, DateTime to) :
               base(dataBaseFolder, new CrossDomainReaderRequest(new FeedCacheKey(symbol, frame), from, to))
        { }

        protected override SeriesStorage<DateTime, QuoteInfo> GetSeriesStorage()
        {
            return _dataBase.GetSeries(
                new DateTimeKeySerializer(),
                TickSerializer.GetSerializer(_request.Key),
                b => b.Time,
                _request.Key.FullInfo,
                true);
        }
    }
}
