using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISymbolKey : IEqualityComparer<ISymbolKey>
    {
        string Name { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }
    }


    public interface ISymbolData : ISymbolKey
    {
        ISymbolKey Key { get; }

        //string Description { get; }

        //string Security { get; }

        bool IsCustom { get; }

        ISymbolInfo Info { get; }

        //ICustomData StorageEntity { get; }

        bool IsDownloadAvailable { get; }

        IVarSet<SymbolStorageSeries> SeriesCollection { get; }


        Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        void WriteSlice(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, Timestamp from, Timestamp to, BarData[] values);

        void WriteSlice(Feed.Types.Timeframe timeFrame, Timestamp from, Timestamp to, QuoteInfo[] values);
    }
}
