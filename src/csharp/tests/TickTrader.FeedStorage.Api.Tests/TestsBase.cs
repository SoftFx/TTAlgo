using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal;
using TickTrader.SeriesStorage.Lmdb;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class TestsBase<TSettings> : IDisposable where TSettings : StorageSettings, new()
    {
        private protected readonly TSettings _settings;
        private protected readonly FeedEmulator _feed;
        private protected readonly ISymbolCatalog _catalog;


        internal abstract SymbolConfig.Types.SymbolOrigin Origin { get; }


        internal TestsBase()
        {
            StorageFactory.InitBinaryStorage((folder, readOnly) => new LmdbManager(folder, readOnly));

            _feed = FeedEmulator.BuildDefaultEmulator();
            _catalog = StorageFactory.BuildCatalog(_feed);
            _settings = new TSettings();
        }


        protected ISymbolKey GetKey(string name) => new StorageSymbolKey(name, Origin);

        protected List<ISymbolData> GetCatalogSymbols(List<ISymbolInfo> smb) => smb.Select(u => _catalog[GetKey(u.Name)]).ToList();


        public async void Dispose()
        {
            await _catalog.CloseCatalog();

            Directory.Delete(_settings.FolderPath, true);
        }
    }
}
