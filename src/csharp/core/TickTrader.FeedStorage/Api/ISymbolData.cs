using ActorSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISymbolKey : IEquatable<ISymbolKey>, IComparable<ISymbolKey>
    {
        string Name { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }
    }


    public interface ISliceInfo
    {
        DateTime From { get; }

        DateTime To { get; }

        int Count { get; }
    }


    public interface ISymbolData : ISymbolKey
    {
        ISymbolKey Key { get; }

        ISymbolInfo Info { get; }


        bool IsCustom { get; }

        bool IsDownloadAvailable { get; }


        IReadOnlyDictionary<ISeriesKey, IStorageSeries> Series { get; }


        event Action<IStorageSeries> SeriesAdded;

        event Action<IStorageSeries> SeriesRemoved;

        event Action<IStorageSeries> SeriesUpdated;


        Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);


        Task<ActorChannel<ISliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        Task<ActorChannel<ISliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to);


        Task<ActorChannel<ISliceInfo>> ImportBarSeriesToStorage(IImportSeriesSettings settings, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide);

        Task<ActorChannel<ISliceInfo>> ImportTickSeriesToStorage(IImportSeriesSettings settings, Feed.Types.Timeframe timeframe);


        Task<IEnumerable<BarData>> GetBarStream(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, DateTime from, DateTime to, bool reversed = false);

        Task<IEnumerable<QuoteInfo>> GetTickStream(Feed.Types.Timeframe timeframe, DateTime from, DateTime to, bool reversed = false);
    }
}