using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISeriesKey
    {
        string FullInfo { get; }

        string Symbol { get; }

        Feed.Types.Timeframe TimeFrame { get; }

        Feed.Types.MarketSide? MarketSide { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }
    }


    public interface IStorageSeries
    {
        ISeriesKey Key { get; }

        DateTime? From { get; }

        DateTime? To { get; }

        double Size { get; }

        event Action<IStorageSeries> SeriesUpdated;


        Task<bool> TryRemove();

        Task<ActorChannel<ISliceInfo>> ExportSeriesToFile(IExportSeriesSettings settings);
    }
}
