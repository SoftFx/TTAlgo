using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.FeedStorage.Api
{
    public interface ISymbolCatalog
    {
        ISymbolData this[ISymbolKey name] { get; }

        IReadOnlyList<ISymbolData> AllSymbols { get; }


        ISymbolCollection OnlineCollection { get; }

        ISymbolCollection CustomCollection { get; }


        Task<ISymbolCatalog> ConnectClient(IOnlineStorageSettings settings);

        Task DisconnectClient();


        Task<ISymbolCatalog> OpenCustomStorage(ICustomStorageSettings settings);

        Task CloseCustomStorage();


        Task CloseCatalog();
    }


    public interface ISymbolCollection
    {
        string StorageFolder { get; }

        ISymbolData this[string name] { get; }


        List<ISymbolData> Symbols { get; }


        Task<bool> TryAddSymbol(ISymbolInfo symbol);

        Task<bool> TryUpdateSymbol(ISymbolInfo symbol);

        Task<bool> TryRemoveSymbol(string symbolName);


        event Action<ISymbolData> SymbolAdded;

        event Action<ISymbolData> SymbolRemoved;

        event Action<ISymbolData, ISymbolData> SymbolUpdated;
    }
}
