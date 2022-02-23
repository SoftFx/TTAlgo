using ActorSharp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    public static class SymbolManagerExtensions
    {
        public static async Task DownloadBarWithObserver(this ISymbolData symbol, IActionObserver observer, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to, bool showStats = true)
        {
            observer.SetMessage($"Downloading bars {symbol.Name} {timeframe} {marketSide}");

            (from, to) = PrepareDate(observer, from, to);

            var barEnumerator = await symbol.DownloadBarSeriesToStorage(timeframe, marketSide, from, to);

            await DownloadChannelHadler(barEnumerator, observer, "bars", showStats);
        }


        public static async Task DownloadTicksWithObserver(this ISymbolData symbol, IActionObserver observer, Feed.Types.Timeframe timeFrame, DateTime from, DateTime to, bool showStats = true)
        {
            observer.SetMessage($"Downloading ticks {symbol.Name} {timeFrame}");

            (from, to) = PrepareDate(observer, from, to);

            var tickEnumerator = await symbol.DownloadTickSeriesToStorage(timeFrame, from, to);

            await DownloadChannelHadler(tickEnumerator, observer, "ticks", showStats);
        }


        private static (DateTime, DateTime) PrepareDate(IActionObserver observer, DateTime from, DateTime to)
        {
            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            observer.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

            return (from, to);
        }

        private static async Task DownloadChannelHadler(ActorChannel<ISliceInfo> channel, IActionObserver observer, string entityName, bool showStats)
        {
            var downloadedCount = 0L;
            var watch = Stopwatch.StartNew();

            try
            {
                while (await channel.ReadNext())
                {
                    var info = channel.Current;

                    if (info.Count > 0)
                    {
                        downloadedCount += info.Count;

                        if (showStats)
                        {
                            var msg = $"Downloading... {downloadedCount} {entityName} are downloaded.";

                            if (watch.ElapsedMilliseconds > 0)
                                msg += $"\nSpeed: {Math.Round(downloadedCount * 1000D / watch.ElapsedMilliseconds)} {entityName} per second";

                            observer.SetMessage(msg);
                        }
                    }

                    observer.SetProgress(info.To.GetAbsoluteDay());

                    if (observer.CancelationToken.IsCancellationRequested)
                    {
                        observer.SetMessage($"Canceled. {downloadedCount} {entityName} were downloaded.");
                        break;
                    }
                }

                observer.SetMessage($"Completed. {downloadedCount} {entityName} were downloaded.");
            }
            catch (Exception ex)
            {
                observer.StopProgress(ex.Message);
            }
            finally
            {
                watch.Stop();
                await channel.Close();
            }
        }
    }
}
