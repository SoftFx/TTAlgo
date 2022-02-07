using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;


namespace TickTrader.FeedStorage
{
    public class OnlineSymbol : SymbolData
    {
        private readonly SymbolInfo _symbolInfo;
        private readonly FeedProvider.Handler _provider;

        public OnlineSymbol(SymbolInfo symbol, FeedProvider.Handler provider) : base(symbol.Name, provider.Cache)
        {
            _symbolInfo = symbol ?? throw new ArgumentNullException("symbol");
            _provider = provider;
        }

        public override bool IsCustom => false;
        public override string Key => "online->" + Name;
        public override string Description => _symbolInfo.Description;
        public override string Security => _symbolInfo.Security;
        public override SymbolInfo InfoEntity => _symbolInfo;
        public override CustomSymbol StorageEntity => CustomSymbol.FromAlgo(_symbolInfo);
        public override bool IsDataAvailable => _provider.FeedProxy.IsAvailable;

        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _provider.FeedProxy.GetAvailableSymbolRange(_symbolInfo.Name, timeFrame, priceType ?? Feed.Types.MarketSide.Bid);
        }

        public async override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            var symbol = _symbolInfo.Name;

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

        public override Task Remove()
        {
            throw new InvalidOperationException("Cannot remove online symbol!");
        }

        public override SymbolToken ToSymbolToken()
        {
            return new SymbolToken(Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
