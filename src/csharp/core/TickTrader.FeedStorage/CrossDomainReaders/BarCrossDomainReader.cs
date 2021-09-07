using System;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    public sealed class BarCrossDomainReader : CrossDomainReader<BarData>
    {
        internal BarCrossDomainReader(string dataBaseFolder, CrossDomainReaderRequest request) : base(dataBaseFolder, request)
        { }

        protected override SeriesStorage<DateTime, BarData> GetSeriesStorage()
        {
            return _dataBase.GetSeries(
                new DateTimeKeySerializer(),
                new BarSerializer(_request.Key.TimeFrame),
                b => b.OpenTime.ToDateTime(),
                _request.Key.CodeString,
                false);
        }
    }
}
