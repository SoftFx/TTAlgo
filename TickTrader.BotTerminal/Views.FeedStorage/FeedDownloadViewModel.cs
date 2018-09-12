using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;

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

            SelectedTimeFrame = varContext.AddProperty(TimeFrames.M1);
            SelectedPriceType = varContext.AddProperty(BarPriceType.Bid);
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

        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
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
        public Property<TimeFrames> SelectedTimeFrame { get; private set; }
        public Property<BarPriceType> SelectedPriceType { get; private set; }

        #endregion

        public void Cancel()
        {
            if (IsBusy.Value)
                DownloadObserver.Cancel();  
            else
                TryClose();
        }

        public void Dispose()
        {
            Symbols.Dispose();
            varContext.Dispose();
        }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);
            Dispose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(!IsBusy.Value);
        }

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
                var range = await  smb.GetAvailableRange(TimeFrames.M1);

                if (SelectedSymbol.Value == smb)
                {
                    DateRange.UpdateBoundaries(range.Item1.Date, range.Item2.Date);
                    IsRangeLoaded.Value = true;
                }
            }
        }

        private async Task<long> DownloadBars(IActionObserver observer, CancellationToken cancelToken, string symbol, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

            var barEnumerator = await _client.FeedHistory.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, from, to);

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

        private async Task<long> DownloadTicks(IActionObserver observer, CancellationToken cancelToken, string symbol, TimeFrames timeFrame, DateTime from, DateTime to)
        {
            var watch = Stopwatch.StartNew();
            long downloadedCount = 0;

            observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

            var tickEnumerator = await _client.FeedHistory.DownloadTickSeriesToStorage(symbol, timeFrame, from, to);

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
            var from = DateRange.From;
            var to = DateRange.To + TimeSpan.FromDays(1);

            observer?.SetMessage("Downloading... \n");

            try
            {
                if (timeFrame.IsTicks())
                    await DownloadTicks(observer, cancelToken, symbol, timeFrame, from, to);
                else
                    await DownloadBars(observer, cancelToken, symbol, timeFrame, priceType, from, to);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
