using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
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
        private DateTime? _rangeFrom;
        private DateTime? _rangeTo;

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
        }

        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public bool IsPriceTypeActual { get; private set; }
        public IObservableListSource<SymbolModel> Symbols { get; }
        public bool CanDownload { get; private set; }
        public double MaxRangeDouble => GetDayNumber(MaxRange);
        public double MinRangeDouble => GetDayNumber(MinRange);
        public DateTime MinRange { get; private set; }
        public DateTime MaxRange { get; private set; }

        #region Observable Properties

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

        public double RangeFromDouble
        {
            get => ToDouble(RangeFrom) ?? 0;
            set { RangeFrom = FromDayNumber(value); }
        }

        public double RangeToDouble
        {
            get => ToDouble(RangeTo) ?? 100;
            set { RangeTo = FromDayNumber(value); }
        }

        public DateTime? RangeFrom
        {
            get => _rangeFrom;
            set
            {
                if (_rangeFrom != value)
                {
                    //if (value < MinRange)
                    //    value = MinRange;
                    //if (value > RangeTo)
                    //    value = RangeTo;

                    _rangeFrom = value;
                    NotifyOfPropertyChange(nameof(RangeFrom));
                    NotifyOfPropertyChange(nameof(RangeFromDouble));
                }
            }
        }

        public DateTime? RangeTo
        {
            get { return _rangeTo; }
            set
            {
                if (_rangeTo != value)
                {
                    //if (value > MaxRange)
                    //    value = MaxRange;
                    //if (value < RangeFrom)
                    //    value = RangeFrom;

                    _rangeTo = value;
                    NotifyOfPropertyChange(nameof(RangeTo));
                    NotifyOfPropertyChange(nameof(RangeToDouble));
                }
            }
        }

        #endregion

        public void Cancel()
        {
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

        public void Download()
        {
        }

        private void UpdateState()
        {
            var connected = _client.IsConnected;
            CanDownload = connected && IsRangeLoaded;
            NotifyOfPropertyChange(nameof(CanDownload));
        }

        private async void UpdateAvailableRange(SymbolModel smb)
        {
            IsRangeLoaded = false;
            RangeFrom = null;
            RangeTo = null;

            var range = await _feedCache.GetAvailableRange(smb.Name, BarPriceType.Bid, TimeFrames.M1);

            if (_selectedSymbol == smb)
            {
                MinRange = range.Item1.Date;
                MaxRange = range.Item2.Date;

                NotifyOfPropertyChange(nameof(MinRange));
                NotifyOfPropertyChange(nameof(MaxRange));
                NotifyOfPropertyChange(nameof(MinRangeDouble));
                NotifyOfPropertyChange(nameof(MaxRangeDouble));

                RangeFrom = MinRange;
                RangeTo = MaxRange;
                IsRangeLoaded = true;
            }
        }

        private double? ToDouble(DateTime? val)
        {
            if (val == null)
                return null;
            return GetDayNumber(val.Value);
        }

        private static double GetDayNumber(DateTime val)
        {
            return (val - DateTime.MinValue).TotalDays;
        }

        private static DateTime FromDayNumber(double day)
        {
            return DateTime.MinValue + TimeSpan.FromDays(Math.Round(day));
        }
    }
}
