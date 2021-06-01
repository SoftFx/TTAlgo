using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account.FeedStorage;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class FeedDownloadViewModel : Screen, IWindowModel, IDisposable
    {
        private readonly VarContext varContext = new VarContext();
        private TraderClientModel _client;

        public FeedDownloadViewModel(TraderClientModel clientModel, SymbolCatalog catalog, SymbolData symbol = null)
        {
            _client = clientModel;

            //Symbols = clientModel.Symbols.Select((k, v) => (SymbolModel)v).OrderBy((k, v) => k).Chain().AsObservable();
            Symbols = catalog.ObservableOnlineSymbols;

            DownloadObserver = new ActionViewModel();
            DateRange = new DateRangeSelectionViewModel();

            SelectedTimeFrame = varContext.AddProperty(Feed.Types.Timeframe.M1);
            SelectedPriceType = varContext.AddProperty(Feed.Types.MarketSide.Bid);
            SelectedSymbol = varContext.AddProperty<SymbolData>(symbol);
            ShowDownloadUi = varContext.AddBoolProperty();

            IsRangeLoaded = varContext.AddBoolProperty();
            IsPriceTypeActual = !SelectedTimeFrame.Var.IsTicks();
            IsBusy = DownloadObserver.IsRunning;

            DownloadEnabled = _client.IsConnected & IsRangeLoaded.Var & !IsBusy;
            CancelEnabled = !IsBusy | DownloadObserver.CanCancel;

            varContext.TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange(SelectedSymbol.Value));

            varContext.TriggerOnChange(IsBusy, a => System.Diagnostics.Debug.WriteLine("IsBusy = " + a.New));
            varContext.TriggerOnChange(DownloadObserver.CanCancel, a => System.Diagnostics.Debug.WriteLine("Observer.CanCancel = " + a.New));

            DisplayName = "Pre-download symbol";
        }

        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames => TimeFrameModel.AllTimeFrames;
        public IEnumerable<Feed.Types.MarketSide> AvailablePriceTypes => EnumHelper.AllValues<Feed.Types.MarketSide>();
        public IObservableList<SymbolData> Symbols { get; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ActionViewModel DownloadObserver { get; }

        #region Observable Properties

        public BoolProperty ShowDownloadUi { get; private set; }
        public BoolProperty IsRangeLoaded { get; private set; }
        public Property<SymbolData> SelectedSymbol { get; private set; }
        public BoolVar DownloadEnabled { get; private set; }
        public BoolVar CancelEnabled { get; private set; }
        public BoolVar IsPriceTypeActual { get; private set; }
        public BoolVar IsBusy { get; private set; }
        public Property<Feed.Types.Timeframe> SelectedTimeFrame { get; private set; }
        public Property<Feed.Types.MarketSide> SelectedPriceType { get; private set; }

        #endregion

        public void Cancel()
        {
            if (IsBusy.Value)
                DownloadObserver.Cancel();  
            else
                TryCloseAsync();
        }

        public void Dispose()
        {
            Symbols.Dispose();
            varContext.Dispose();
        }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            Dispose();
            return base.TryCloseAsync(dialogResult);
        }

        //public override void TryClose(bool? dialogResult = default(bool?))
        //{
        //    base.TryClose(dialogResult);
        //    Dispose();
        //}

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!IsBusy.Value);
        }

        //public override void CanClose(Action<bool> callback)
        //{
        //    callback(!IsBusy.Value);
        //}

        public void Download()
        {
            ShowDownloadUi.Value = true;
            DownloadObserver.Start(DownloadAsync);
        }

        private async void UpdateAvailableRange(SymbolData smb)
        {
            IsRangeLoaded.Value = false;
            DateRange.Reset();

            if (smb != null)
            {
                var range = await  smb.GetAvailableRange(Feed.Types.Timeframe.M1);

                if (SelectedSymbol.Value == smb)
                {
                    DateTime from = DateTime.UtcNow.Date;
                    DateTime to = from;

                    if (range.Item1 != null && range.Item2 != null)
                    {
                        from = range.Item1.Value;
                        to = range.Item2.Value;
                    }

                    DateRange.UpdateBoundaries(from, to);
                    IsRangeLoaded.Value = true;
                }
            }
        }

        private async Task<long> DownloadBars(IActionObserver observer, CancellationToken cancelToken, string symbol, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            observer?.StartProgress(fromUtc.GetAbsoluteDay(), toUtc.GetAbsoluteDay());

            var barEnumerator = await _client.FeedHistory.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, fromUtc, toUtc);

            try
            {
                var watch = Stopwatch.StartNew();
                long downloadedCount = 0;

                while (await barEnumerator.ReadNext())
                {
                    var info = barEnumerator.Current;
                    if (info.Count > 0)
                    {
                        downloadedCount += info.Count;
                        var msg = "Downloading... " + downloadedCount + " bars are downloaded.";
                        if (watch.ElapsedMilliseconds > 0)
                            msg += "\nBar per second: " + Math.Round(((double)downloadedCount * 1000) / watch.ElapsedMilliseconds);
                        observer.SetMessage(msg);
                    }

                    observer?.SetProgress(info.To.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        await barEnumerator.Close();
                        observer.SetMessage("Canceled. " + downloadedCount + " bars were downloaded.");
                        return downloadedCount;
                    }
                }

                observer.SetMessage("Completed. " + downloadedCount + " bars were downloaded.");

                return downloadedCount;
            }
            finally
            {
                await barEnumerator.Close();
            }
        }

        private async Task<long> DownloadTicks(IActionObserver observer, CancellationToken cancelToken, string symbol, Feed.Types.Timeframe timeFrame, DateTime from, DateTime to)
        {
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var watch = Stopwatch.StartNew();
            long downloadedCount = 0;

            observer?.StartProgress(fromUtc.GetAbsoluteDay(), toUtc.GetAbsoluteDay());

            var tickEnumerator = await _client.FeedHistory.DownloadTickSeriesToStorage(symbol, timeFrame, fromUtc, toUtc);

            try
            {
                while (await tickEnumerator.ReadNext())
                {
                    var info = tickEnumerator.Current;
                    if (info.Count > 0)
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
                        observer.SetMessage("Canceled! " + downloadedCount + " ticks were downloaded.");
                        return downloadedCount;
                    }
                }

                observer.SetMessage("Completed: " + downloadedCount + " ticks were downloaded.");

                return downloadedCount;
            }
            finally
            {
                 await tickEnumerator.Close();
            }
        }

        private async Task DownloadAsync(IActionObserver observer, CancellationToken cancelToken)
        {
            var symbol = SelectedSymbol.Value.Name;
            var timeFrame = SelectedTimeFrame.Value;
            var priceType = SelectedPriceType.Value;
            var from = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(DateRange.To + TimeSpan.FromDays(1), DateTimeKind.Utc);

            observer?.SetMessage("Downloading... \n");

            try
            {
                if (timeFrame.IsTicks())
                    await DownloadTicks(observer, cancelToken, symbol, timeFrame, from, to);
                else
                    await DownloadBars(observer, cancelToken, symbol, timeFrame, priceType, from, to);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
