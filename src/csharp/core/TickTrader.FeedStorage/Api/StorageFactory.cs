using System;
using System.Threading.Tasks;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage.Api
{
    public static class StorageFactory
    {
        private static Func<string, bool, IBinaryStorageManager> _factoryFunc;


        public static void InitBinaryStorage(Func<string, bool, IBinaryStorageManager> factoryFunc) => _factoryFunc = factoryFunc;

        internal static IBinaryStorageManager BuildBinaryStorage(string baseFolder, bool readOnly = false) => _factoryFunc?.Invoke(baseFolder, readOnly);


        public static ISymbolCatalog BuildCatalog(IClientFeedProvider feedProvider)
        {
            return new SymbolCatalog(feedProvider);
        }


        public static Task<ISymbolCatalog> BuildCatalogAndConnect(IClientFeedProvider feedProvider, IOnlineStorageSettings onlineStorageSettings)
        {
            return BuildCatalog(feedProvider).ConnectClient(onlineStorageSettings);
        }

        public static Task<ISymbolCatalog> BuildCatalogAndOpen(IClientFeedProvider feedProvider, ICustomStorageSettings customStorageSettingss)
        {
            return BuildCatalog(feedProvider).OpenCustomStorage(customStorageSettingss);
        }

        public static Task<ISymbolCatalog> BuildCatalogOpenAndConnect(IClientFeedProvider feedProvider, ICustomStorageSettings customStorageSettings,
                                                                      IOnlineStorageSettings onlineStorageSettings)
        {
            return BuildCatalog(feedProvider).OpenCustomStorage(customStorageSettings).ContinueWith(t => t.Result.ConnectClient(onlineStorageSettings)).Unwrap();
        }
    }
}
