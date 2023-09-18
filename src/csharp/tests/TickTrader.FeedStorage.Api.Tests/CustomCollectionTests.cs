using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public class CustomCollectionTests : ManageSymbolsTestsBase<CustomStorageSettings>
    {
        internal override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Custom;

        internal override ISymbolCollection Collection => _catalog.CustomCollection;


        public override async Task InitializeAsync()
        {
            await _catalog.OpenCustomStorage(_settings);
        }


        [Fact(Timeout = TestTimeout)]
        public void Default_Inilialization()
        {
            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.CustomCollection.Symbols);
            Assert.Equal(_catalog.CustomCollection.StorageFolder, _settings.GetExpectedPath());
        }

        [Fact(Timeout = TestTimeout)]
        public async Task Close_CustomStorage()
        {
            await _catalog.CloseCustomStorage();

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(_catalog.CustomCollection.Symbols);
            Assert.Equal(_catalog.CustomCollection.StorageFolder, string.Empty);
        }

        [Fact(Timeout = TestTimeout)]
        public async Task Close_Full_CustomStorage()
        {
            await Add_Symbols(100);
            await Close_CustomStorage();
        }

        [Theory(Timeout = TestTimeout)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Reconnect_CustomStorage(int count)
        {
            var newSymbols = await LoadSymbolsToCatalog(count, Collection.TryAddSymbol);

            await Close_CustomStorage();
            await _catalog.OpenCustomStorage(_settings);

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, _catalog.CustomCollection.Symbols.Count);

            AssertOrderCollection(_catalog.CustomCollection.Symbols, newSymbols);
            AssertOrderCollection(GetCatalogSymbols(newSymbols), newSymbols);
        }
    }
}
