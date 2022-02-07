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
        ISymbolData this[ISymbolKey name] { get; }

        IReadOnlyList<ISymbolData> AllSymbols { get; }


        ISymbolCollection<ISymbolData> OnlineCollection { get; }

        ISymbolCollection<ISymbolData> CustomCollection { get; }


        Task<ISymbolCatalog> ConnectClient(IOnlineStorageSettings settings);

        Task<ISymbolCatalog> OpenCustomStorage(ICustomStorageSettings settings);

        Task CloseCustomStorage();


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


        event Action<T> SymbolAdded;

        event Action<T> SymbolRemoved;

        event Action<T, T> SymbolUpdated;
    }

    public interface ISymbolKey : IEqualityComparer<ISymbolKey>
    {
        string Name { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }

        string Id { get; }
    }


    public interface ISymbolData
    {
        ISymbolKey StorageKey { get; }

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
