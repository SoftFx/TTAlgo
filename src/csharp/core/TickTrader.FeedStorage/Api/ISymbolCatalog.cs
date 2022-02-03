using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISymbolCatalog
    {
        SymbolData this[string name] { get; }

        List<SymbolData> AllSymbols { get; }


        ISymbolCollection<SymbolData> OnlineCollection { get; }

        ISymbolCollection<CustomSymbol> CustomCollection { get; }


        Task<ISymbolCatalog> Connect(IOnlineStorageSettings settings);

        Task Close();


        Task<ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        Task<ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(string symbol, Feed.Types.Timeframe timeframe, DateTime from, DateTime to);
    }


    public interface ISymbolCollection<T> where T : ISymbolData
    {
        string StorageFolder { get; }

        T this[string name] { get; }


        List<T> Symbols { get; }


        bool TryAddSymbol(T symbol);

        bool TryUpdateSymbol(T symbol);

        bool TryRemoveSymbol(T symbol);
    }


    public interface ISymbolData
    {
        string Name { get; }
        string Description { get; }
        string Key { get; }
        string Security { get; }
        bool IsCustom { get; }
        SymbolInfo InfoEntity { get; }
        CustomSymbol StorageEntity { get; }
        bool IsDataAvailable { get; }

        IVarSet<SymbolStorageSeries> SeriesCollection { get; }

        SymbolToken ToSymbolToken();


        Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        void WriteSlice(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, Timestamp from, Timestamp to, BarData[] values);

        void WriteSlice(Feed.Types.Timeframe timeFrame, Timestamp from, Timestamp to, QuoteInfo[] values);
    }
}
