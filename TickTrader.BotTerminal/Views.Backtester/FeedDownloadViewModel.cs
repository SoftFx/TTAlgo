using Caliburn.Micro;
using Machinarium.Qnil;
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
        private TimeFrames _selectedTimeFrame;
        private BarPriceType _selectedPriceType;
        private SymbolModel _selectedSymbol;
        private bool _isRangeLoaded;
        private FeedHistoryProviderModel _feedCache;
        private TraderClientModel _client;
        private bool _showDownloadUi;
        private CancellationTokenSource _cancelDownloadSrc;
        private Task _downloadTask;

        public FeedDownloadViewModel(TraderClientModel clientModel)
        {
            _client = clientModel;
            _feedCache = clientModel.History;
            clientModel.Connected += UpdateState;
            clientModel.Disconnected += UpdateState;

            Symbols = clientModel.Symbols.Select((k, v) => (SymbolModel)v).OrderBy((k, v) => k).Chain().AsObservable();

            DisplayName = "Pre-download symbol";
            SelectedTimeFrame = TimeFrames.M1;
            SelectedPriceType = BarPriceType.Bid;

            DownloadObserver = new ProgressViewModel();
            DateRange = new DateRangeSelectionViewModel();

            UpdateState();
        }

        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public bool IsPriceTypeActual { get; private set; }
        public IObservableListSource<SymbolModel> Symbols { get; }
        public DateRangeSelectionViewModel DateRange { get; }
        public bool CanDownload { get; private set; }
        public bool CanCancel { get; private set; }
        public ProgressViewModel DownloadObserver { get; }
        public bool IsInputEnabled => !IsDowloading;

        private bool IsDowloading => _cancelDownloadSrc != null;
        private bool IsCanceling => _cancelDownloadSrc?.IsCancellationRequested ?? false;

        #region Observable Properties

        public bool ShowDownloadUi
        {
            get => _showDownloadUi;
            set
            {
                if (_showDownloadUi != value)
                {
                    _showDownloadUi = value;
                    NotifyOfPropertyChange(nameof(ShowDownloadUi));
                }
            }
        }

        public bool IsRangeLoaded
        {
            get => _isRangeLoaded;
            set
            {
                if (_isRangeLoaded != value)
                {
                    _isRangeLoaded = value;
                    NotifyOfPropertyChange(nameof(IsRangeLoaded));
                    UpdateState();
                }
            }
        }

        public SymbolModel SelectedSymbol
        {
            get => _selectedSymbol;
            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    NotifyOfPropertyChange(nameof(SelectedSymbol));

                    if (_selectedSymbol != null)
                        UpdateAvailableRange(_selectedSymbol);

                    DownloadObserver.Reset();
                }
            }
        }

        public TimeFrames SelectedTimeFrame
        {
            get => _selectedTimeFrame;
            set
            {
                if (_selectedTimeFrame != value)
                {
                    _selectedTimeFrame = value;
                    NotifyOfPropertyChange(nameof(SelectedTimeFrame));

                    IsPriceTypeActual = _selectedTimeFrame != TimeFrames.Ticks;
                    NotifyOfPropertyChange(nameof(IsPriceTypeActual));
                }
            }
        }

        public BarPriceType SelectedPriceType
        {
            get => _selectedPriceType;
            set
            {
                if (_selectedPriceType != value)
                {
                    _selectedPriceType = value;
                    NotifyOfPropertyChange(nameof(SelectedPriceType));
                }
            }
        }

        #endregion

        public async void Cancel()
        {
            if (IsDowloading)
            {
                _cancelDownloadSrc.Cancel();
                UpdateState();
                await _downloadTask;
            }
            else
                TryClose();
        }

        public void Dispose()
        {
            Symbols.Dispose();
            _client.Connected -= UpdateState;
            _client.Disconnected -= UpdateState;
        }

        public override void TryClose(bool? dialogResult = default(bool?))
        {
            base.TryClose(dialogResult);
            Dispose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(!IsDowloading);
        }

        public void Download()
        {
            _downloadTask = DownloadAsync();
        }

        private void UpdateState()
        {
            CanDownload = _client.IsConnected && IsRangeLoaded && !IsDowloading;
            CanCancel = !IsCanceling;
            NotifyOfPropertyChange(nameof(CanDownload));
            NotifyOfPropertyChange(nameof(CanCancel));
            NotifyOfPropertyChange(nameof(IsInputEnabled));
        }

        private async void UpdateAvailableRange(SymbolModel smb)
        {
            IsRangeLoaded = false;
            DateRange.From = null;
            DateRange.To = null;

            var range = await _feedCache.GetAvailableRange(smb.Name, BarPriceType.Bid, TimeFrames.M1);

            if (_selectedSymbol == smb)
            {
                DateRange.UpdateBoundaries(range.Item1.Date, range.Item2.Date);
                IsRangeLoaded = true;
            }
        }

        private async Task DownloadAsync()
        {
            ShowDownloadUi = true;
            DownloadObserver.Reset();
            _cancelDownloadSrc = new CancellationTokenSource();
            UpdateState();

            try
            {
                await _feedCache.Downlaod(SelectedSymbol.Name, SelectedTimeFrame, SelectedPriceType,
                    DateRange.From.Value, DateRange.To.Value, _cancelDownloadSrc.Token, DownloadObserver);
            }
            catch (Exception ex)
            {

            }

            _cancelDownloadSrc = null;
            UpdateState();
        }
    }
}
