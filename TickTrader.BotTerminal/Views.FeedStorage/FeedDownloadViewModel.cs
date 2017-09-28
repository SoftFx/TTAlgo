using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
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
        private readonly Property<CancellationTokenSource> _cancelDownloadSrc;
        private Task _downloadTask;
        private readonly BoolVar isDowloading;

        public FeedDownloadViewModel(TraderClientModel clientModel, SymbolModel symbol = null)
        {
            _client = clientModel;
            _cancelDownloadSrc = varContext.AddProperty<CancellationTokenSource>();

            Symbols = clientModel.Symbols.Select((k, v) => (SymbolModel)v).OrderBy((k, v) => k).Chain().AsObservable();

            DownloadObserver = new ProgressViewModel();
            DateRange = new DateRangeSelectionViewModel();

            SelectedTimeFrame = varContext.AddProperty(TimeFrames.M1);
            SelectedPriceType = varContext.AddProperty(BarPriceType.Bid);
            SelectedSymbol = varContext.AddProperty<SymbolModel>(symbol);
            varContext.TriggerOnChange(SelectedSymbol.Var, a => DownloadObserver.Reset());
            ShowDownloadUi = varContext.AddBoolProperty();

            IsRangeLoaded = varContext.AddBoolProperty();
            IsPriceTypeActual = SelectedTimeFrame.Var != TimeFrames.Ticks;

            isDowloading = _cancelDownloadSrc.Var != (CancellationTokenSource)null;
            var isCanceling = isDowloading & _cancelDownloadSrc.Var.Check(c => c != null && c.IsCancellationRequested);

            IsBusy = isDowloading;

            DownloadEnabled = _client.IsConnected & IsRangeLoaded.Var & !isDowloading;
            CancelEnabled = !isCanceling;

            varContext.TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange(SelectedSymbol.Value));

            DisplayName = "Pre-download symbol";
        }

        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public IObservableListSource<SymbolModel> Symbols { get; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ProgressViewModel DownloadObserver { get; }


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

        public async void Cancel()
        {
            if (isDowloading.Value)
            {
                _cancelDownloadSrc.Value.Cancel();
                await _downloadTask;
            }
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
            callback(!isDowloading.Value);
        }

        public void Download()
        {
            _downloadTask = DownloadAsync();
        }

        private async void UpdateAvailableRange(SymbolModel smb)
        {
            IsRangeLoaded.Value = false;
            DateRange.From = null;
            DateRange.To = null;

            var range = await _client.History.GetAvailableRange(smb.Name, BarPriceType.Bid, TimeFrames.M1);

            if (SelectedSymbol.Value == smb)
            {
                DateRange.UpdateBoundaries(range.Item1.Date, range.Item2.Date);
                IsRangeLoaded.Value = true;
            }
        }

        private async Task DownloadAsync()
        {
            ShowDownloadUi.Value = true;
            DownloadObserver.Reset();
            _cancelDownloadSrc.Value = new CancellationTokenSource();

            try
            {
                await _client.History.Downlaod(SelectedSymbol.Value.Name, SelectedTimeFrame.Value, SelectedPriceType.Value,
                    DateRange.From.Value, DateRange.To.Value, _cancelDownloadSrc.Value.Token, DownloadObserver);
            }
            catch (Exception ex)
            {

            }

            _cancelDownloadSrc.Value = null;
        }
    }
}
