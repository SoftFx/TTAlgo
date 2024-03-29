﻿using ActorSharp;
using Machinarium.Qnil;
using Machinarium.Var;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSymbolSetupViewModel : EntityBase
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ISymbolCatalog _catalog;

        private IntProperty _requestsCount;
        private Property<string> _errorProp;
        private bool _suppressRangeUpdates;

        public BacktesterSymbolSetupViewModel(SymbolSetupType type, ISymbolCatalog catalog, Var<ISymbolData> smbSource = null)
        {
            _catalog = catalog;

            _requestsCount = AddIntProperty();
            _errorProp = AddProperty<string>();

            SetupType = type;

            AvailableSymbols = new ObservableCollection<ISymbolData>(catalog.AllSymbols);
            AvailableSymbols.CollectionChanged += Symbols_CollectionChanged;

            if (type == SymbolSetupType.Main)
                AvailableTimeFrames = TimeFrameModel.BarTimeFrames;
            else
                AvailableTimeFrames = TimeFrameModel.AllTimeFrames;

            SelectedTimeframe = AddProperty<Feed.Types.Timeframe>();
            SelectedPriceType = AddProperty<DownloadPriceChoices>();
            SelectedSymbol = AddValidable<ISymbolData>();
            SelectedSymbolName = AddValidable<string>();
            AvailableBases = AddProperty<List<Feed.Types.Timeframe>>();

            SelectedSymbolName.AddValidationRule((string str) => AvailableSymbols.FirstOrDefault(u => u.Name == str) != null, NotFoundMessage);

            if (!(smbSource is null))
            {
                SelectedSymbol.Var = smbSource;
                SelectedSymbolName.Value = SelectedSymbol.Value.Name;
            }

            AvailableRange = AddProperty<Tuple<DateTime, DateTime>>();
            IsUpdating = _requestsCount.Var > 0;

            SelectedTimeframe.Value = Feed.Types.Timeframe.M1;
            SelectedPriceType.Value = DownloadPriceChoices.Both;

            var isTicks = SelectedTimeframe.Var == Feed.Types.Timeframe.Ticks
                | SelectedTimeframe.Var == Feed.Types.Timeframe.TicksLevel2;

            CanChangePrice = !isTicks;

            IsSymbolSelected = SelectedSymbol.Var.IsNotNull();

            if (type != SymbolSetupType.Main)
            {
                TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange(SelectedTimeframe.Value));
                TriggerOnChange(SelectedTimeframe.Var, a => UpdateAvailableRange(SelectedTimeframe.Value));
            }

            TriggerOnChange(SelectedSymbolName.Var, a => UpdateSelectedSymbol(type));

            //TriggerOn(SelectedSymbol.Var.IsNull(), SelectDefaultSymbol);
            TriggerOn(isTicks, () => SelectedPriceType.Value = DownloadPriceChoices.Both);

            SelectDefaultSymbol();

            IsValid = IsSymbolSelected & Error.IsEmpty() & SelectedSymbolName.ErrorVar.IsNull() & !IsUpdating;
        }

        public SymbolSetupType SetupType { get; private set; }
        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames { get; }
        public IEnumerable<DownloadPriceChoices> AvailablePriceTypes => EnumHelper.AllValues<DownloadPriceChoices>();
        public Property<List<Feed.Types.Timeframe>> AvailableBases { get; }
        public ObservableCollection<ISymbolData> AvailableSymbols { get; }
        public Validable<string> SelectedSymbolName { get; }
        public Validable<ISymbolData> SelectedSymbol { get; }
        public Property<Feed.Types.Timeframe> SelectedTimeframe { get; }
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

        public string NotFoundMessage => $"Symbol {SelectedSymbolName.Value} not found!";

        public string AsText()
        {
            var smb = (SymbolInfo)SelectedSymbol.Value.Info;
            var swapLong = smb.Swap.Enabled ? smb.Swap.SizeLong : 0;
            var swapShort = smb.Swap.Enabled ? smb.Swap.SizeShort : 0;

            return string.Format("{0} {1}, commission={2} {3}, swapLong={4} swapShort={5} ",
                smb.Name, SelectedTimeframe.Value, smb.Commission, smb.Commission.ValueType, swapLong, swapShort);
        }

        public void UpdateSelectedSymbol(SymbolSetupType type)
        {
            _requestsCount.Value++;

            var smb = AvailableSymbols.FirstOrDefault(u => u.Name == SelectedSymbolName.Value);
            _errorProp.Value = null;

            if (smb != null)
                SelectedSymbol.Value = smb;
            else
            {
                AvailableRange.Value = null;
                _errorProp.Value = NotFoundMessage;
            }

            _requestsCount.Value--;
        }

        public async Task UpdateAvailableRange(Feed.Types.Timeframe timeFrame)
        {
            if (_suppressRangeUpdates)
                return;

            var smb = SelectedSymbol.Value;

            if (smb != null && smb.IsDownloadAvailable)
            {
                _requestsCount.Value++;

                try
                {
                    var range = await smb.GetAvailableRange(timeFrame, Feed.Types.MarketSide.Bid);
                    if (range.Item1 != null && range.Item2 != null)
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

        public Task PrecacheData(IActionObserver observer, DateTime fromLimit, DateTime toLimit)
        {
            return PrecacheData(observer, fromLimit, toLimit, SelectedTimeframe.Value);
        }

        public async Task PrecacheData(IActionObserver observer, DateTime fromLimit, DateTime toLimit, Feed.Types.Timeframe timeFrameChoice)
        {
            if (observer.CancelationToken.IsCancellationRequested)
                return;

            if (SetupType == SymbolSetupType.Main)
                return;

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
                observer.ShowCustomMessages = false;

                if (timeFrameChoice.IsTicks())
                    await smb.DownloadTicksWithObserver(observer, timeFrameChoice, precacheFrom, precacheTo);
                else
                {
                    if (priceChoice == DownloadPriceChoices.Bid | priceChoice == DownloadPriceChoices.Both)
                        await smb.DownloadBarWithObserver(observer, timeFrameChoice, Feed.Types.MarketSide.Bid, precacheFrom, precacheTo);

                    if (priceChoice == DownloadPriceChoices.Ask | priceChoice == DownloadPriceChoices.Both)
                        await smb.DownloadBarWithObserver(observer, timeFrameChoice, Feed.Types.MarketSide.Ask, precacheFrom, precacheTo);
                }

                observer.ShowCustomMessages = true;
            }
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

            if (SetupType != SymbolSetupType.Main)
                UpdateAvailableRange(SelectedTimeframe.Value);
        }

        public void PrintCacheData(Feed.Types.Timeframe timeFrameChoice)
        {
            var smb = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;

            //if (smb == null)
            //    return;

            //if (timeFrameChoice == Feed.Types.Timeframe.Ticks || timeFrameChoice == Feed.Types.Timeframe.TicksLevel2)
            //    smb.PrintCacheData(timeFrameChoice, null);
            //else
            //{
            //    smb.PrintCacheData(timeFrameChoice, Feed.Types.MarketSide.Bid);
            //    smb.PrintCacheData(timeFrameChoice, Feed.Types.MarketSide.Ask);
            //}
        }

        //public void InitSeriesBuilder(Backtester tester)
        //{
        //    tester.Feed.AddBarBuilder(SelectedSymbol.Value.Name, SelectedTimeframe.Value, Feed.Types.MarketSide.Bid);
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
            SelectedSymbolName.Value = AvailableSymbols.Select(u => u.Name).FirstOrDefault();
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
    internal enum SymbolSetupType { Main, Additional, MainShadow }
}
