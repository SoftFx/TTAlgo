using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestEnviroment;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public class OnlineCollectionTests : TestsBase
    {
        private readonly OnlineStorageSettings _settings;


        public OnlineCollectionTests() : base()
        {
            _settings = new OnlineStorageSettings();

            Init();

            _catalog.ConnectClient(_settings).Wait();
        }


        [Fact]
        public void Default_Initialization()
        {
            var expected = _feed.DefaultSymbol;

            Assert.Single(_catalog.AllSymbols);
            Assert.Single(_catalog.OnlineCollection.Symbols);
            Assert.Equal(_catalog.OnlineCollection.StorageFolder, _settings.GetExpectedPath());

            AssertOnlineSymbols(_catalog[GetOnlineKey(expected.Name)], expected);
            AssertOnlineSymbols(_catalog.OnlineCollection[expected.Name], expected);
        }

        [Fact]
        public async Task Disconnect_Client()
        {
            await _catalog.DisconnectClient();

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.OnlineCollection.Symbols);
            Assert.Equal(_catalog.OnlineCollection.StorageFolder, string.Empty);
        }

        [Fact]
        public async Task Reconnect_Client()
        {
            await Disconnect_Client();
            await _catalog.ConnectClient(_settings);

            Default_Initialization();
        }

        [Fact]
        public async Task Remove_DefaultSymbol()
        {
            var expected = _feed.DefaultSymbol;
            ISymbolData received = null;

            void RemoveSymbolHandler(ISymbolData smb) => received = smb;

            _catalog.OnlineCollection.SymbolRemoved += RemoveSymbolHandler;

            var result = await _catalog.OnlineCollection.TryRemoveSymbol(expected.Name);

            _catalog.OnlineCollection.SymbolRemoved -= RemoveSymbolHandler;

            Assert.True(result);
            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.OnlineCollection.Symbols);

            AssertOnlineSymbols(received, expected);
        }

        [Fact]
        public async Task Update_DefaultSymbol()
        {
            ISymbolData receivedOld = null;
            ISymbolData receivedNew = null;

            var oldSymbol = _feed.DefaultSymbol;
            var newSymbol = _feed.GetUpdatedSymbol(oldSymbol);

            void UpdateSymbolHandler(ISymbolData old, ISymbolData @new)
            {
                receivedOld = old;
                receivedNew = @new;
            }

            _catalog.OnlineCollection.SymbolUpdated += UpdateSymbolHandler;

            var result = await _catalog.OnlineCollection.TryUpdateSymbol(newSymbol);

            _catalog.OnlineCollection.SymbolUpdated -= UpdateSymbolHandler;

            Assert.True(result);
            Assert.Single(_catalog.AllSymbols);
            Assert.Single(_catalog.OnlineCollection.Symbols);

            AssertOnlineSymbols(_catalog[GetOnlineKey(newSymbol.Name)], newSymbol);
            AssertOnlineSymbols(_catalog.OnlineCollection[newSymbol.Name], newSymbol);

            AssertOnlineSymbols(receivedOld, oldSymbol);
            AssertOnlineSymbols(receivedNew, newSymbol);
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

            _catalog.OnlineCollection.SymbolAdded += AddSymbolHandler;

            var newSymbols = await LoadSymbolsToCatalog(count);

            _catalog.OnlineCollection.SymbolAdded -= AddSymbolHandler;

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, _catalog.OnlineCollection.Symbols.Count);

            AssertCollection(received, newSymbols);
            AssertCollection(_catalog.OnlineCollection.Symbols, newSymbols);
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
            var newSymbols = await LoadSymbolsToCatalog(count);

            void RemoveSymbolHandler(ISymbolData smb) => received.Add(smb);

            _catalog.OnlineCollection.SymbolRemoved += RemoveSymbolHandler;

            foreach (var smb in newSymbols)
                await _catalog.OnlineCollection.TryRemoveSymbol(smb.Name);

            _catalog.OnlineCollection.SymbolRemoved -= RemoveSymbolHandler;

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.OnlineCollection.Symbols);

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

            _catalog.OnlineCollection.SymbolUpdated += UpdateSymbolHandle;

            var updates = count;
            var newSymbols = (await LoadSymbolsToCatalog(count)).ToDictionary(u => u.Name);

            while (updates-- > 0)
            {
                var smb = newSymbols.ElementAt(RandomGenerator.GetRandomInt(0, count)).Value;
                var updatedSmb = _feed.GetUpdatedSymbol(smb);

                await _catalog.OnlineCollection.TryUpdateSymbol(updatedSmb);

                newSymbols[smb.Name] = updatedSmb;
            }

            _catalog.OnlineCollection.SymbolUpdated -= UpdateSymbolHandle;

            var newSymbolsList = newSymbols.Values.ToList();

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, _catalog.OnlineCollection.Symbols.Count);

            AssertCollection(GetCatalogSymbols(newSymbolsList), newSymbolsList);
            AssertCollection(_catalog.OnlineCollection.Symbols, newSymbolsList);
            AssertOrderCollection(received.Values, newSymbols.Values.Where(u => received.ContainsKey(u.Name)));
        }

        [Theory]
        [InlineData(FeedStorageFolderOptions.NoHierarchy)]
        [InlineData(FeedStorageFolderOptions.ServerHierarchy)]
        [InlineData(FeedStorageFolderOptions.ServerClientHierarchy)]
        public async Task Check_Folder_Hierarchy(FeedStorageFolderOptions options)
        {
            await _catalog.DisconnectClient();

            _settings.Options = options;

            await _catalog.ConnectClient(_settings);

            var expectedPath = _settings.GetExpectedPath();

            Assert.Equal(_catalog.OnlineCollection.StorageFolder, expectedPath);
            Assert.True(Directory.Exists(expectedPath));
        }

        private async Task<List<ISymbolInfo>> LoadSymbolsToCatalog(int count)
        {
            await _catalog.OnlineCollection.TryRemoveSymbol(_feed.DefaultSymbol.Name);

            var newSymbols = new Dictionary<string, ISymbolInfo>(count);

            while (newSymbols.Count < count)
            {
                var smb = _feed.GetRandomSymbol();

                if (newSymbols.ContainsKey(smb.Name))
                    continue;

                newSymbols[smb.Name] = smb;
                await _catalog.OnlineCollection.TryAddSymbol(smb);
            }

            return newSymbols.Values.ToList();
        }

        private static void AssertOnlineSymbols(ISymbolData actual, ISymbolInfo expected)
        {
            Assert.NotNull(actual);
            Assert.False(actual.IsCustom);
            Assert.True(actual.IsDownloadAvailable);
            Assert.NotNull(actual.SeriesCollection);
            Assert.Equal(actual.Name, expected.Name);
            Assert.Same(actual.Info, expected);

            AssertSymbolKey(actual.Key, GetOnlineKey(expected.Name));
        }

        private static void AssertOrderCollection(IEnumerable<ISymbolData> actual, IEnumerable<ISymbolInfo> expected)
        {
            AssertCollection(actual.OrderBy(u => u.Name).ToList(), expected.OrderBy(u => u.Name).ToList());
        }

        private static void AssertCollection(List<ISymbolData> actual, List<ISymbolInfo> expected)
        {
            Assert.Equal(actual.Count, expected.Count);

            for (int i = 0; i < expected.Count; ++i)
                AssertOnlineSymbols(actual[i], expected[i]);
        }

        private List<ISymbolData> GetCatalogSymbols(List<ISymbolInfo> smb) => smb.Select(u => _catalog[GetOnlineKey(u.Name)]).ToList();

        private static ISymbolKey GetOnlineKey(string name) => new StorageSymbolKey(name, SymbolConfig.Types.SymbolOrigin.Online);


        public override void Dispose()
        {
            base.Dispose();

            Directory.Delete(_settings.FolderPath, true);
        }
    }
}
