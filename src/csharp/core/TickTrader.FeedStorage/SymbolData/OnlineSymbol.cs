using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;


namespace TickTrader.FeedStorage
{
    internal sealed class OnlineSymbol : BaseSymbol
    {
        private readonly FeedProvider.Handler _provider;


        public override bool IsDownloadAvailable => _provider.FeedProxy.IsAvailable;

        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Online;


        public OnlineSymbol(ISymbolInfo symbol, FeedProvider.Handler provider) : base(symbol, provider.Cache)
        {
            _provider = provider;
        }


        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _provider.FeedProxy.GetAvailableSymbolRange(Name, timeFrame, priceType ?? Feed.Types.MarketSide.Bid);
        }

        public override Task<ActorSharp.ActorChannel<SliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to)
        {
            return _provider?.DownloadBarSeriesToStorage(Name, timeframe, marketSide, from, to);
        }

        public override Task<ActorSharp.ActorChannel<SliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to)
        {
            return _provider.DownloadTickSeriesToStorage(Name, timeframe, from, to);
        }


        public async override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            var symbol = Name;

            var watch = Stopwatch.StartNew();
            int downloadedCount = 0;

            if (!timeFrame.IsTicks())
            {
                observer?.SetMessage("Downloading " + symbol + " " + priceType);

                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var barEnumerator = await _provider.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, from, to);

                while (await barEnumerator.ReadNext())
                {
                    var info = barEnumerator.Current;
                    if (info.Count > 0)
                    {
                        downloadedCount += info.Count;
                        if (showStats)
                        {
                            var msg = "Downloading... " + downloadedCount + " bars are downloaded.";
                            if (watch.ElapsedMilliseconds > 0)
                                msg += "\nBar per second: " + Math.Round(((double)downloadedCount * 1000) / watch.ElapsedMilliseconds);
                            observer.SetMessage(msg);
                        }
                    }

                    observer?.SetProgress(info.To.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        await barEnumerator.Close();
                        observer.SetMessage("Canceled. " + downloadedCount + " bars were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed. " + downloadedCount + " bars were downloaded.");
            }
            else // ticks
            {
                //var endDay = to.GetAbsoluteDay();
                //var totalDays = endDay - from.GetAbsoluteDay();

                observer?.SetMessage("Downloading " + symbol);

                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var tickEnumerator = await _provider.DownloadTickSeriesToStorage(symbol, timeFrame, from, to);

                while (await tickEnumerator.ReadNext())
                {
                    var info = tickEnumerator.Current;
                    if (showStats && info.Count > 0)
                    {
                        downloadedCount += info.Count;
                        var msg = "Downloading... " + downloadedCount + " ticks are downloaded.";
                        if (watch.ElapsedMilliseconds > 0)
                            msg += "\nTicks per second: " + Math.Round(((double)downloadedCount * 1000) / watch.ElapsedMilliseconds);
                        observer.SetMessage(msg);
                    }

                    observer?.SetProgress(info.From.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        await tickEnumerator.Close();
                        observer.SetMessage("Canceled! " + downloadedCount + " ticks were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed: " + downloadedCount + " ticks were downloaded.");
            }
        }
    }
}
