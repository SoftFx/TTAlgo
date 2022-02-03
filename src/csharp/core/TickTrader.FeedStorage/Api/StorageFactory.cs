using System.Threading.Tasks;

namespace TickTrader.FeedStorage.Api
{
    public static class StorageFactory
    {
        public static ISymbolCatalog BuildCatalog(IClientFeedProvider feedProvider, ICustomStorageSettings settings)
        {
            return new SymbolCatalog(feedProvider, settings);
        }

        public static Task<ISymbolCatalog> BuildCatalogAndConnect(IClientFeedProvider feedProvider,
                                                                  ICustomStorageSettings customStorageSettings,
                                                                  IOnlineStorageSettings onlineStorageSettings)
        {
            return BuildCatalog(feedProvider, customStorageSettings).Connect(onlineStorageSettings);
        }
    }
}
