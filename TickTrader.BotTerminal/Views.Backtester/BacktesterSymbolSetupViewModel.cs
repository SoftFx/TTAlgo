using ActorSharp;
using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSymbolSetupViewModel : EntityBase
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IntProperty _requestsCount;
        private Property<string> _errorProp;
        private bool _suppressRangeUpdates;

        public BacktesterSymbolSetupViewModel(SymbolSetupType type, IObservableList<SymbolData> symbols, Var<SymbolData> smbSource = null)
        {
            _requestsCount = AddIntProperty();
            _errorProp = AddProperty<string>();

            SetupType = type;

            AvailableSymbols = symbols;

            symbols.CollectionChanged += Symbols_CollectionChanged;

            if (type == SymbolSetupType.Main)
                AvailableTimeFrames = TimeFrameModel.BarTimeFrames;
            else
                AvailableTimeFrames = TimeFrameModel.AllTimeFrames;

            SelectedTimeframe = AddProperty<TimeFrames>();
            SelectedPriceType = AddProperty<DownloadPriceChoices>();
            SelectedSymbol = AddValidable<SymbolData>();
            AvailableBases = AddProperty<List<TimeFrames>>();

            if ((object)smbSource != null)
                SelectedSymbol.Var = smbSource;

            AvailableRange = AddProperty<Tuple<DateTime, DateTime>>();
            IsUpdating = _requestsCount.Var > 0;

            SelectedTimeframe.Value = TimeFrames.M1;
            SelectedPriceType.Value = DownloadPriceChoices.Both;

            var isTicks = SelectedTimeframe.Var == TimeFrames.Ticks
                | SelectedTimeframe.Var == TimeFrames.TicksLevel2;

            CanChangePrice = !isTicks;

            IsSymbolSelected = SelectedSymbol.Var.IsNotNull();

            if (type == SymbolSetupType.Additional)
            {
                TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange(SelectedTimeframe.Value));
                TriggerOnChange(SelectedTimeframe.Var, a => UpdateAvailableRange(SelectedTimeframe.Value));
            }

            //TriggerOn(SelectedSymbol.Var.IsNull(), SelectDefaultSymbol);
            TriggerOn(isTicks, () => SelectedPriceType.Value = DownloadPriceChoices.Both);

            SelectDefaultSymbol();

            IsValid = IsSymbolSelected & Error.IsEmpty() & !IsUpdating;
        }

        public SymbolSetupType SetupType { get; private set; }
        public IEnumerable<TimeFrames> AvailableTimeFrames { get; }
        public IEnumerable<DownloadPriceChoices> AvailablePriceTypes => EnumHelper.AllValues<DownloadPriceChoices>();
        public Property<List<TimeFrames>> AvailableBases { get; }
        public IObservableList<SymbolData> AvailableSymbols { get; }
        public Validable<SymbolData> SelectedSymbol { get; }
        public Property<TimeFrames> SelectedTimeframe { get; }
        public Property<DownloadPriceChoices> SelectedPriceType { get; }
        public Property<Tuple<DateTime, DateTime>> AvailableRange { get; }
        public BoolVar IsUpdating { get; }
        public BoolVar CanChangePrice { get; }
        public BoolVar IsValid { get; }
        public BoolVar IsSymbolSelected { get; }
        public Var<string> Error => _errorProp.Var;

        public void Add() => OnAdd?.Invoke();
        public void Remove() => Removed?.Invoke(this);

        public event System.Action<BacktesterSymbolSetupViewModel> Removed;
        public event System.Action OnAdd;

        public string AsText()
        {
            var smb = SelectedSymbol.Value.InfoEntity;
            var swapLong = smb.SwapEnabled ? smb.SwapSizeLong : 0;
            var swapShort = smb.SwapEnabled ? smb.SwapSizeShort : 0;

            return string.Format("{0} {1}, commission={2} {3}, swapLong={4} swapShort={5} ",
                smb.Name, SelectedTimeframe.Value, smb.Commission, smb.CommissionType, swapLong, swapShort);
        }

        public async void UpdateAvailableRange(TimeFrames timeFrame)
        {
            if (_suppressRangeUpdates)
                return;

            var smb = SelectedSymbol.Value;

            if (smb != null)
            {
                _requestsCount.Value++;

                try
                {
                    var range = await smb.GetAvailableRange(timeFrame, BarPriceType.Bid);
                    if (range != null && range.Item1 != null && range.Item2 != null)
                    {
                        AvailableRange.Value = new Tuple<DateTime, DateTime>(range.Item1.Value.Date, range.Item2.Value.Date + TimeSpan.FromDays(1));
                        _errorProp.Value = null;
                    }
                    else
                    {
                        AvailableRange.Value = null;
                        _errorProp.Value = "No data available!";
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn("Failed to get available range for symbol + " + smb.Name + ": " + ex.Message);
                    _errorProp.Value = "An error occurred while requesting available data range! See log file for more details!";
                }

                _requestsCount.Value--;
            }
        }

        public Task PrecacheData(IActionObserver observer, CancellationToken cToken, DateTime fromLimit, DateTime toLimit)
        {
            return PrecacheData(observer, cToken, fromLimit, toLimit, SelectedTimeframe.Value);
        }

        public async Task PrecacheData(IActionObserver observer, CancellationToken cToken, DateTime fromLimit, DateTime toLimit, TimeFrames timeFrameChoice)
        {
            //if (SetupType == SymbolSetupType.Main)
            //    return;

            if (SelectedSymbol.Value == null)
                return;

            var precacheFrom = GetLocalFrom(fromLimit);
            var precacheTo = GetLocalTo(toLimit);

            if (precacheFrom > precacheTo)
                return;

            var smb = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;

            if (!smb.IsCustom)
            {
                if (timeFrameChoice == TimeFrames.Ticks || timeFrameChoice == TimeFrames.TicksLevel2)
                {
                    // ticks
                    await smb.DownloadToStorage(observer, false, cToken, timeFrameChoice, BarPriceType.Bid, precacheFrom, precacheTo);
                }
                else // bars
                {
                    if (priceChoice == DownloadPriceChoices.Bid | priceChoice == DownloadPriceChoices.Both)
                        await smb.DownloadToStorage(observer, false, cToken, timeFrameChoice, BarPriceType.Bid, precacheFrom, precacheTo);

                    if (priceChoice == DownloadPriceChoices.Ask | priceChoice == DownloadPriceChoices.Both)
                        await smb.DownloadToStorage(observer, false, cToken, timeFrameChoice, BarPriceType.Ask, precacheFrom, precacheTo);
                }
            }
        }

        public void Apply(Optimizer tester, DateTime fromLimit, DateTime toLimit)
        {
            Apply(tester.CommonSettings, tester.Feed, fromLimit, toLimit, SelectedTimeframe.Value);
        }

        public void Apply(Optimizer tester, DateTime fromLimit, DateTime toLimit, TimeFrames baseTimeFrame)
        {
            Apply(tester.CommonSettings, tester.Feed, fromLimit, toLimit, baseTimeFrame);
        }

        public void Apply(Backtester tester, DateTime fromLimit, DateTime toLimit, bool isVisualizing)
        {
            Apply(tester.CommonSettings, tester.Feed, fromLimit, toLimit, SelectedTimeframe.Value);
            SetupDataOutput(tester, isVisualizing);
        }

        public void Apply(Backtester tester, DateTime fromLimit, DateTime toLimit, TimeFrames baseTimeFrame, bool isVisualizing)
        {
            Apply(tester.CommonSettings, tester.Feed, fromLimit, toLimit, baseTimeFrame);
            SetupDataOutput(tester, isVisualizing);
        }

        public void Apply(CommonTestSettings settings, FeedEmulator feedEmulator, DateTime fromLimit, DateTime toLimit, bool isVisualizing)
        {
            Apply(settings, feedEmulator, fromLimit, toLimit, SelectedTimeframe.Value);
        }

        public void Apply(CommonTestSettings settings, FeedEmulator feedEmulator, DateTime fromLimit, DateTime toLimit, TimeFrames baseTimeFrame)
        {
            var smbData = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;

            if (smbData == null)
                return;

            if (SetupType == SymbolSetupType.Main)
            {
                settings.MainSymbol = smbData.Name;
                settings.MainTimeframe = SelectedTimeframe.Value; // SelectedTimeframe may differ from baseTimeFrame in case of main symbol
            }

            var precacheFrom = GetLocalFrom(fromLimit);
            var precacheTo = GetLocalTo(toLimit);

            settings.Symbols.Add(smbData.Name, smbData.InfoEntity);

            if (baseTimeFrame == TimeFrames.Ticks || baseTimeFrame == TimeFrames.TicksLevel2)
            {
                ITickStorage feed = smbData.GetCrossDomainTickReader(baseTimeFrame, precacheFrom, precacheTo);

                feedEmulator.AddSource(smbData.Name, feed);
            }
            else
            {
                IBarStorage bidFeed = null;
                IBarStorage askFeed = null;

                if (priceChoice == DownloadPriceChoices.Bid | priceChoice == DownloadPriceChoices.Both)
                    bidFeed = smbData.GetCrossDomainBarReader(baseTimeFrame, BarPriceType.Bid, precacheFrom, precacheTo);

                if (priceChoice == DownloadPriceChoices.Ask | priceChoice == DownloadPriceChoices.Both)
                    askFeed = smbData.GetCrossDomainBarReader(baseTimeFrame, BarPriceType.Ask, precacheFrom, precacheTo);

                feedEmulator.AddSource(smbData.Name, baseTimeFrame, bidFeed, askFeed);
            }
        }

        private void SetupDataOutput(Backtester tester, bool isVisualizing)
        {
            var smbData = SelectedSymbol.Value;

            if (isVisualizing)
                tester.SymbolDataConfig.Add(smbData.Name, TestDataSeriesFlags.Stream | TestDataSeriesFlags.Realtime);
            else if (SetupType == SymbolSetupType.Main)
                tester.SymbolDataConfig.Add(smbData.Name, TestDataSeriesFlags.Stream);
        }

        public void Reset()
        {
            _suppressRangeUpdates = true;

            try
            {

                if (SelectedSymbol.Value == null || !AvailableSymbols.Contains(SelectedSymbol.Value))
                    SelectDefaultSymbol();
            }
            finally
            {
                _suppressRangeUpdates = false;
            }

            if (SetupType == SymbolSetupType.Additional)
                UpdateAvailableRange(SelectedTimeframe.Value);
        }

        public void PrintCacheData(TimeFrames timeFrameChoice)
        {
            var smb = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;

            if (smb == null)
                return;

            if (timeFrameChoice == TimeFrames.Ticks || timeFrameChoice == TimeFrames.TicksLevel2)
                smb.PrintCacheData(timeFrameChoice, null);
            else
            {
                smb.PrintCacheData(timeFrameChoice, BarPriceType.Bid);
                smb.PrintCacheData(timeFrameChoice, BarPriceType.Ask);
            }
        }

        //public void InitSeriesBuilder(Backtester tester)
        //{
        //    tester.Feed.AddBarBuilder(SelectedSymbol.Value.Name, SelectedTimeframe.Value, BarPriceType.Bid);
        //}

        private DateTime GetLocalFrom(DateTime fromLimit)
        {
            var availableFrom = AvailableRange.Value.Item1;

            if (fromLimit >= availableFrom)
                return fromLimit;

            return availableFrom;
        }

        private DateTime GetLocalTo(DateTime toLimit)
        {
            var availableTo = AvailableRange.Value.Item2;

            if (toLimit <= availableTo)
                return toLimit;

            return availableTo;
        }

        private void SelectDefaultSymbol()
        {
            SelectedSymbol.Value = AvailableSymbols.FirstOrDefault();
        }

        public override void Dispose()
        {
            if (AvailableSymbols != null)
                AvailableSymbols.CollectionChanged -= Symbols_CollectionChanged;

            base.Dispose();
        }

        private void Symbols_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (SelectedSymbol.Value == null || e.OldItems.Contains(SelectedSymbol.Value))
                    SelectDefaultSymbol();
            }
        }

        //private IEnumerable<BarEntity> ReadSlices(BlockingChannel<Slice<DateTime, BarEntity>> channel)
        //{
        //    Slice<DateTime, BarEntity> slice;
        //    while (channel.Read(out slice))
        //    {
        //        foreach (var bar in slice.Content)
        //            yield return bar;
        //    }
        //}
    }

    internal enum DownloadPriceChoices { Bid, Ask, Both }
    internal enum SymbolSetupType { Main, Additional }
}
