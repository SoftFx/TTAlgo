using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
//using TickTrader.FeedStorage;

namespace TickTrader.Algo.Account
{
    public class FeedHistoryProviderModel : Actor
    {
        private IAlgoLogger logger;

        //private string _dataFolder;
        //private FeedHistoryFolderOptions _folderOptions;
        private IFeedServerApi _feedProxy;

        private void Init(/*HistoryProviderSettings settings,*/ string loggerId)
        {
            logger = AlgoLoggerFactory.GetLogger<FeedHistoryProviderModel>(loggerId);
            //_dataFolder = settings.FolderPath;
            //_folderOptions = settings.Options;
        }

        internal class ControlHandler : Handler<FeedHistoryProviderModel>
        {
            public ControlHandler(/*HistoryProviderSettings settings, */string loggerId) : base(SpawnLocal<FeedHistoryProviderModel>())
            {
                Actor.Send(a => a.Init(/*settings,*/ loggerId));
            }

            public Task Start(IFeedServerApi api, string server, string login) => Actor.Call(a => a.Start(api, server, login));
            public Task Stop() => Actor.Call(a => a.Stop());

            public Ref<FeedHistoryProviderModel> Ref => Actor;
        }

        public class Handler : Handler<FeedHistoryProviderModel>
        {
            public Handler(Ref<FeedHistoryProviderModel> aRef) : base(aRef) { }

            //public FeedCache.Handler Cache { get; private set; }

            public async Task Init()
            {
                //Cache = await Actor.Call(a => a.Cache);
                //await Cache.SyncData();
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<BarData>> GetBarList(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
            {
                return Actor.Call(a => a.GetBarList(symbol, marketSide, timeframe, from, to));
            }

            /// Warning: This method downloads all bars into a collection of unlimmited size! Use wisely!
            public Task<List<QuoteInfo>> GetQuoteList(string symbol, Timestamp from, Timestamp to, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuoteList(symbol, from, to, includeLevel2));
            }

            public Task<BarData[]> GetBarPage(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp startTime, int count)
            {
                return Actor.Call(a => a.GetBarPage(symbol, marketSide, timeframe, startTime, count));
            }

            public Task<QuoteInfo[]> GetQuotePage(string symbol, Timestamp startTime, int count, bool includeLevel2)
            {
                return Actor.Call(a => a.GetQuotePage(symbol, startTime, count, includeLevel2));
            }

            public Task<(DateTime?, DateTime?)> GetAvailableRange(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
            {
                return Actor.Call(a => a.GetAvailableRange(symbol, marketSide, timeframe));
            }
        }

        //protected FeedCache.Handler Cache => _diskCache;

        private async Task Start(IFeedServerApi feed, string server, string login)
        {
            _feedProxy = feed;

            //var onlineFolder = _dataFolder;
            //if (_folderOptions == FeedHistoryFolderOptions.ServerHierarchy || _folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
            //    onlineFolder = Path.Combine(onlineFolder, PathHelper.Escape(server));
            //if (_folderOptions == FeedHistoryFolderOptions.ServerClientHierarchy)
            //    onlineFolder = Path.Combine(onlineFolder, PathHelper.Escape(login));

            //await _diskCache.SyncData();
            //await _diskCache.Start(onlineFolder);
        }

        private async Task Stop()
        {
            try
            {
                _feedProxy = null;
                //await _diskCache.Stop();
                //_diskCache.Dispose(); old
            }
            catch (Exception ex)
            {
                logger.Error("Init ERROR " + ex.ToString());
            }
        }

        private Task<(DateTime?, DateTime?)> GetAvailableRange(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe)
        {
            return _feedProxy?.GetAvailableRange(symbol, marketSide, timeframe) ?? Task.FromResult<(DateTime?, DateTime?)>(default);
        }

        private async Task<BarData[]> GetBarPage(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int pageSize)
        {
            var pages = new List<BarData[]>();

            var isBackward = pageSize < 0;
            pageSize = Math.Abs(pageSize);

            while (pageSize > 0)
            {
                if (!isBackward && from > DateTime.UtcNow.ToTimestamp())
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadBarPage(symbol, from, isBackward ? -pageSize : pageSize, marketSide, timeframe);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                pageSize -= page.Length;

                from = TimeMs.ToTimestamp(isBackward
                    ? page.First().OpenTime - 1
                    : page.Last().CloseTime + 1);
            }

            return pages.ConcatAll();
        }

        private async Task<QuoteInfo[]> GetQuotePage(string symbol, Timestamp from, int count, bool includeLevel2)
        {
            var pages = new List<QuoteInfo[]>();

            var isBackward = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                if (!isBackward && from > DateTime.UtcNow.ToTimestamp())
                    break; // we get last bar somehow even it is out of our requested frame

                var page = await _feedProxy.DownloadQuotePage(symbol, from, isBackward ? -count : count, includeLevel2);

                if (page.Length == 0)
                    break;

                pages.Add(page);
                count -= page.Length;

                from = TimeMs.ToTimestamp(isBackward
                    ? page.First().UtcMs - 1
                    : page.Last().UtcMs + 1);
            }

            return pages.ConcatAll();
        }

        private async Task<List<BarData>> GetBarList(string symbol, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            var result = new List<BarData>();

            while (true)
            {
                var page = await _feedProxy.DownloadBarPage(symbol, from, 4000, marketSide, timeframe);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded bar page {0} : {1} ({2} {3} {4})", from, page.Length, symbol, marketSide, timeframe);

                var toMs = TimeMs.FromTimestamp(to);
                foreach (var bar in page)
                {
                    if (bar.OpenTime <= toMs)
                    {
                        result.Add(bar);
                        from = TimeMs.ToTimestamp(bar.CloseTime);
                    }
                    else
                        return result;
                }

                if (page.Length < 5)
                    return result;
            }
        }

        private async Task<List<QuoteInfo>> GetQuoteList(string symbol, Timestamp from, Timestamp to, bool includeLevel2)
        {
            var result = new List<QuoteInfo>();

            var toTicks = TimeTicks.FromTimestamp(to);
            while (true)
            {
                var page = await _feedProxy.DownloadQuotePage(symbol, from, 4000, includeLevel2);

                if (page == null || page.Length == 0)
                    return result;

                logger.Debug("Downloaded quote page {0} : {1} ({2} {3})", from, page.Length, symbol, includeLevel2 ? "l2" : "top");

                var fromTicks = TimeTicks.FromTimestamp(from);
                foreach (var quote in page)
                {
                    if (quote.UtcTicks <= toTicks)
                    {
                        result.Add(quote);
                        fromTicks = quote.UtcTicks;
                    }
                    else
                        return result;
                }

                if (page.Length < 5)
                    return result;

                from = TimeTicks.ToTimestamp(fromTicks);
            }
        }
    }
}
