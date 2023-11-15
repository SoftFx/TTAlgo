using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public class OnlineCollectionTests : ManageSymbolsTestsBase<OnlineStorageSettings>
    {
        internal override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Online;

        internal override ISymbolCollection Collection => _catalog.OnlineCollection;


        public override async Task InitializeAsync()
        {
            await _catalog.ConnectClient(_settings);
        }


        [Fact(Timeout = TestTimeout)]
        public void Default_Initialization()
        {
            var expected = _feed.DefaultSymbol;

            Assert.Single(_catalog.AllSymbols);
            Assert.Single(_catalog.OnlineCollection.Symbols);
            Assert.Equal(_catalog.OnlineCollection.StorageFolder, _settings.GetExpectedPath());

            AssertSymbols(_catalog[GetKey(expected.Name)], expected);
            AssertSymbols(_catalog.OnlineCollection[expected.Name], expected);
        }

        [Fact(Timeout = TestTimeout)]
        public async Task Disconnect_Client()
        {
            await _catalog.DisconnectClient();

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.OnlineCollection.Symbols);
            Assert.Equal(_catalog.OnlineCollection.StorageFolder, string.Empty);
        }

        [Fact(Timeout = TestTimeout)]
        public async Task Reconnect_Client()
        {
            await Disconnect_Client();
            await _catalog.ConnectClient(_settings);

            Default_Initialization();
        }

        [Theory(Timeout = TestTimeout)]
        [InlineData(StorageFolderOptions.NoHierarchy)]
        [InlineData(StorageFolderOptions.ServerHierarchy)]
        [InlineData(StorageFolderOptions.ServerClientHierarchy)]
        public async Task Check_Folder_Hierarchy(StorageFolderOptions options)
        {
            await _catalog.DisconnectClient();

            _settings.Options = options;

            await _catalog.ConnectClient(_settings);

            var expectedPath = _settings.GetExpectedPath();

            Assert.Equal(_catalog.OnlineCollection.StorageFolder, expectedPath);
            Assert.True(Directory.Exists(expectedPath));
        }

        [Fact(Timeout = TestTimeout)]
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

            AssertSymbols(received, expected);
        }

        [Fact(Timeout = TestTimeout)]
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

            AssertSymbols(_catalog[GetKey(newSymbol.Name)], newSymbol);
            AssertSymbols(_catalog.OnlineCollection[newSymbol.Name], newSymbol);

            AssertSymbols(receivedOld, oldSymbol);
            AssertSymbols(receivedNew, newSymbol);
        }
    }
}
