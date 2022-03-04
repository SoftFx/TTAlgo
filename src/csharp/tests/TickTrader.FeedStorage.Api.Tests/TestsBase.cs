using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestEnviroment;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal;
using TickTrader.SeriesStorage.Lmdb;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class TestsBase<TSettings> : IDisposable where TSettings : StorageSettings, new()
    {
        private protected readonly TSettings _settings;
        private protected readonly FeedEmulator _feed;
        private protected readonly ISymbolCatalog _catalog;


        internal abstract SymbolConfig.Types.SymbolOrigin Origin { get; }

        internal abstract ISymbolCollection Collection { get; }


        internal TestsBase()
        {
            StorageFactory.InitBinaryStorage((folder, readOnly) => new LmdbManager(folder, readOnly));

            _feed = FeedEmulator.BuildDefaultEmulator();
            _catalog = StorageFactory.BuildCatalog(_feed);
            _settings = new TSettings();
        }


        protected ISymbolKey GetKey(string name) => new StorageSymbolKey(name, Origin);

        protected List<ISymbolData> GetCatalogSymbols(List<ISymbolInfo> smb) => smb.Select(u => _catalog[GetKey(u.Name)]).ToList();

        protected async Task<List<ISymbolInfo>> LoadSymbolsToCatalog(int count, Func<ISymbolInfo, Task> addFunc)
        {
            await Collection.TryRemoveSymbol(_feed.DefaultSymbol.Name);

            var newSymbols = new Dictionary<string, ISymbolInfo>(count);

            while (newSymbols.Count < count)
            {
                var smb = _feed.GetRandomSymbol();

                if (newSymbols.ContainsKey(smb.Name))
                    continue;

                newSymbols[smb.Name] = smb;
                await addFunc(smb);
            }

            return newSymbols.Values.ToList();
        }


        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Add_Symbols(int count)
        {
            var received = new List<ISymbolData>(count);

            void AddSymbolHandler(ISymbolData smb) => received.Add(smb);

            Collection.SymbolAdded += AddSymbolHandler;

            var newSymbols = await LoadSymbolsToCatalog(count, Collection.TryAddSymbol);

            Collection.SymbolAdded -= AddSymbolHandler;

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, Collection.Symbols.Count);

            AssertCollection(received, newSymbols);
            AssertCollection(Collection.Symbols, newSymbols);
            AssertCollection(GetCatalogSymbols(newSymbols), newSymbols);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Remove_Symbols(int count)
        {
            var received = new List<ISymbolData>(count);
            var newSymbols = await LoadSymbolsToCatalog(count, Collection.TryAddSymbol);

            void RemoveSymbolHandler(ISymbolData smb) => received.Add(smb);

            Collection.SymbolRemoved += RemoveSymbolHandler;

            foreach (var smb in newSymbols)
                await Collection.TryRemoveSymbol(smb.Name);

            Collection.SymbolRemoved -= RemoveSymbolHandler;

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(Collection.Symbols);

            AssertCollection(received, newSymbols);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Updates_Symbols(int count)
        {
            var received = new Dictionary<string, ISymbolData>(count);

            void UpdateSymbolHandle(ISymbolData _, ISymbolData @new) => received[@new.Name] = @new;

            Collection.SymbolUpdated += UpdateSymbolHandle;

            var updates = count;
            var newSymbols = (await LoadSymbolsToCatalog(count, Collection.TryAddSymbol)).ToDictionary(u => u.Name);

            while (updates-- > 0)
            {
                var smb = newSymbols.ElementAt(RandomGenerator.GetRandomInt(0, count)).Value;
                var updatedSmb = _feed.GetUpdatedSymbol(smb);

                await Collection.TryUpdateSymbol(updatedSmb);

                newSymbols[smb.Name] = updatedSmb;
            }

            Collection.SymbolUpdated -= UpdateSymbolHandle;

            var newSymbolsList = newSymbols.Values.ToList();

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, Collection.Symbols.Count);

            AssertCollection(GetCatalogSymbols(newSymbolsList), newSymbolsList);
            AssertCollection(Collection.Symbols, newSymbolsList);
            AssertOrderCollection(received.Values, newSymbols.Values.Where(u => received.ContainsKey(u.Name)));
        }


        protected void AssertOrderCollection(IEnumerable<ISymbolData> actual, IEnumerable<ISymbolInfo> expected)
        {
            AssertCollection(actual.OrderBy(u => u.Name).ToList(), expected.OrderBy(u => u.Name).ToList());
        }

        protected void AssertCollection(List<ISymbolData> actual, List<ISymbolInfo> expected)
        {
            Assert.Equal(actual.Count, expected.Count);

            for (int i = 0; i < expected.Count; ++i)
                AssertSymbols(actual[i], expected[i]);
        }

        protected void AssertSymbols(ISymbolData actual, ISymbolInfo expected)
        {
            Assert.NotNull(actual);
            Assert.Equal(actual.Origin, Origin);
            Assert.True(actual.IsDownloadAvailable);
            Assert.NotNull(actual.SeriesCollection);
            Assert.Equal(actual.Name, expected.Name);

            AssertSymbolInfo(actual.Info, expected);
            AssertSymbolKey(actual.Key, GetKey(expected.Name));
        }

        protected static void AssertSymbolKey(ISymbolKey actual, ISymbolKey expected)
        {
            Assert.Equal(actual.Name, expected.Name);
            Assert.Equal(actual.Origin, expected.Origin);
        }

        protected static void AssertSymbolInfo(ISymbolInfo actual, ISymbolInfo expected)
        {
            var type = typeof(ISymbolInfo);

            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var actualValue = pi.GetValue(actual);
                var expectedValue = pi.GetValue(expected);

                Assert.NotNull(actualValue);
                Assert.NotNull(expectedValue);
                Assert.Equal(actualValue, expectedValue);
            }
        }


        public void Dispose()
        {
            _catalog.CloseCatalog();

            Directory.Delete(_settings.FolderPath, true);
        }
    }
}
