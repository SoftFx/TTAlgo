using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISymbolKey : IEqualityComparer<ISymbolKey>, IComparable<ISymbolKey>
    {
        string Name { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }
    }


    public interface ISymbolData : ISymbolKey
    {
        ISymbolKey Key { get; }

        ISymbolInfo Info { get; }


        bool IsCustom { get; }

        bool IsDownloadAvailable { get; }

        List<IStorageSeries> SeriesCollection { get; }


        event Action<IStorageSeries> SeriesAdded;

        event Action<IStorageSeries> SeriesRemoved;


        Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to);


        void WriteSlice(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, Timestamp from, Timestamp to, BarData[] values);

        void WriteSlice(Feed.Types.Timeframe timeFrame, Timestamp from, Timestamp to, QuoteInfo[] values);
    }
}
