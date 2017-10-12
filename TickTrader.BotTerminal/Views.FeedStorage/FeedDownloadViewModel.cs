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

        public FeedDownloadViewModel(TraderClientModel clientModel, SymbolModel symbol = null)
        {
            _client = clientModel;

            Symbols = clientModel.Symbols.Select((k, v) => (SymbolModel)v).OrderBy((k, v) => k).Chain().AsObservable();

            DownloadObserver = new ActionViewModel();
            DateRange = new DateRangeSelectionViewModel();

            SelectedTimeFrame = varContext.AddProperty(TimeFrames.M1);
            SelectedPriceType = varContext.AddProperty(BarPriceType.Bid);
            SelectedSymbol = varContext.AddProperty<SymbolModel>(symbol);
            ShowDownloadUi = varContext.AddBoolProperty();

            IsRangeLoaded = varContext.AddBoolProperty();
            IsPriceTypeActual = SelectedTimeFrame.Var != TimeFrames.Ticks;
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
        public IObservableListSource<SymbolModel> Symbols { get; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ActionViewModel DownloadObserver { get; }

        #region Observable Properties

        public BoolProperty ShowDownloadUi { get; private set; }
        public BoolProperty IsRangeLoaded { get; private set; }
        public Property<SymbolModel> SelectedSymbol { get; private set; }
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

        private async void UpdateAvailableRange(SymbolModel smb)
        {
            IsRangeLoaded.Value = false;
            DateRange.From = null;
            DateRange.To = null;

            if (smb != null)
            {
                var range = await _client.History.GetAvailableRange(smb.Name, BarPriceType.Bid, TimeFrames.M1);

                if (SelectedSymbol.Value == smb)
                {
                    DateRange.UpdateBoundaries(range.Item1.Date, range.Item2.Date);
                    IsRangeLoaded.Value = true;
                }
            }
        }

        private async Task DownloadAsync(IActionObserver observer, CancellationToken cancelToken)
        {
            var symbol = SelectedSymbol.Value.Name;
            var timeFrame = SelectedTimeFrame.Value;
            var priceType = SelectedPriceType.Value;
            var from = DateRange.From.Value;
            var to = DateRange.To.Value;

            observer?.SetMessage("Downloading... \n");

            var watch = Stopwatch.StartNew();
            int downloadedCount = 0;

            if (timeFrame != TimeFrames.Ticks)
            {
                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var barEnumerator = _client.History.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, from, to);

                while (await barEnumerator.Next())
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
                        observer.SetMessage("Canceled. " + downloadedCount + " bars were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed. " + downloadedCount + " bars were downloaded.");
            }
            else
            {
                var endDay = to.GetAbsoluteDay();
                var totalDays = endDay - from.GetAbsoluteDay();

                observer?.StartProgress(0, totalDays);

                var tickEnumerator = _client.History.DownloadTickSeriesToStorage(symbol, false, from, to);

                while (await tickEnumerator.Next())
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

                    observer?.SetProgress(endDay - info.From.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        observer.SetMessage("Canceled! " + downloadedCount + " ticks were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed: " + downloadedCount + " ticks were downloaded.");
            }
        }
    }
}
