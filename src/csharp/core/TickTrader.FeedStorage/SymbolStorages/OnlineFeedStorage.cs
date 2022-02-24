using ActorSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal sealed partial class OnlineFeedStorage : FeedStorageBase
    {
        private IFeedLoader _barLoader, _tickLoader;
        private IClientFeedProvider _feedProvider;


        private void InitLocalData(IClientFeedProvider feedProvider)
        {
            _feedProvider = feedProvider;

            _barLoader = new BarFeedLoader(this);
            _tickLoader = new TickFeedLoader(this);
        }


        private void DownloadSeriesToStorage(ActorChannel<ISliceInfo> stream, FeedCacheKey key, DateTime from, DateTime to)
        {
            LoadSeriesData(stream, key, from, to, key.TimeFrame.IsTick() ? _tickLoader : _barLoader);
        }


        private async void LoadSeriesData(ActorChannel<ISliceInfo> buffer, FeedCacheKey key, DateTime from, DateTime to, IFeedLoader loader)
        {
            try
            {
                var cur = from;

                foreach (var cacheItem in IterateCacheKeysInternal(key, from, to))
                {
                    if (cacheItem.From > cur)
                        cur = await loader.DownloadData(buffer, key, cur, cacheItem.From);

                    if (!await buffer.Write(new SliceInfo(cacheItem.From, cacheItem.To, 0)))
                        return;

                    cur = cacheItem.To;
                }

                if (cur < to)
                    cur = await loader.DownloadData(buffer, key, cur, to);
            }
            catch (Exception ex)
            {
                await buffer.Close(ex);
            }
            finally
            {
                await buffer.Close();
            }
        }


        internal sealed class Handler : FeedHandler
        {
            private readonly Ref<OnlineFeedStorage> _ref;


            internal IClientFeedProvider FeedProxy { get; private set; }


            public Handler(Ref<OnlineFeedStorage> actorRef) : base(actorRef.Cast<OnlineFeedStorage, FeedStorageBase>())
            {
                _ref = actorRef;
            }


            public async Task Start(IClientFeedProvider feed, IOnlineStorageSettings settings)
            {
                FeedProxy = feed;

                await _ref.Call(a => a.InitLocalData(feed));
                await Start(GenerateLocalPath(settings));
            }


            protected override Task SyncSymbolCollection()
            {
                foreach (var s in FeedProxy.Symbols)
                    _symbols.Add(s.Name, new OnlineSymbol(s, this));

                return Task.CompletedTask;
            }

            public override Task<bool> TryAddSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }

            public override Task<bool> TryRemoveSymbol(string symbol)
            {
                throw new System.NotImplementedException();
            }

            public override Task<bool> TryUpdateSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }


            public async Task<ActorChannel<ISliceInfo>> DownloadSeriesToStorage(FeedCacheKey key, DateTime from, DateTime to)
            {
                var channel = ActorChannel.NewOutput<ISliceInfo>();
                await _ref.OpenChannel(channel, (a, c) => a.DownloadSeriesToStorage(c, key, from, to));
                return channel;
            }


            private static string GenerateLocalPath(IOnlineStorageSettings settings)
            {
                var dataFolder = settings.FolderPath;
                var folderOptions = settings.Options;

                switch (folderOptions)
                {
                    case FeedStorageFolderOptions.NoHierarchy:
                        return Path.Combine(dataFolder, PathHelper.Escape(settings.Server));
                    case FeedStorageFolderOptions.ServerHierarchy:
                    case FeedStorageFolderOptions.ServerClientHierarchy:
                        return Path.Combine(dataFolder, PathHelper.Escape(settings.Login));
                    default:
                        return dataFolder;
                }
            }
        }
    }
}
