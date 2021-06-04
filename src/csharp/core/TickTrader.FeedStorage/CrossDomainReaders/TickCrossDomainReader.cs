using System;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    public sealed class TickCrossDomainReader : CrossDomainReader<QuoteInfo>
    {
        public TickCrossDomainReader(string dataBaseFolder, CrossDomainReaderRequest request) : base(dataBaseFolder, request)
        { }

        protected override SeriesStorage<DateTime, QuoteInfo> GetSeriesStorage()
        {
            return _dataBase.GetSeries(
                new DateTimeKeySerializer(),
                TickSerializer.GetSerializer(_request.Key),
                b => b.Time,
                _request.Key.CodeString,
                true);
        }
    }
}
