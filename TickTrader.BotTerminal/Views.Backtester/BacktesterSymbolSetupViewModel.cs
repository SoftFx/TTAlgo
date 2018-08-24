using ActorSharp;
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
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSymbolSetupViewModel : EntityBase
    {
        private IntProperty _requestsCount;

        public BacktesterSymbolSetupViewModel(SymbolSetupType type, IObservableList<SymbolData> symbols, Var<SymbolData> smbSource = null)
        {
            _requestsCount = AddIntProperty();

            SetupType = type;

            AvailableSymbols = symbols;

            SelectedTimeframe = AddProperty<TimeFrames>();
            SelectedPriceType = AddProperty<DownloadPriceChoices>();
            SelectedSymbol = AddProperty<SymbolData>();
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

            if (type != SymbolSetupType.MainSymbol)
            {
                TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange());
                //TriggerOnChange(SelectedTimeframe.Var, a => UpdateAvailableRange());
                //TriggerOnChange(SelectedPriceType.Var, a => UpdateAvailableRange());
            }

            TriggerOn(isTicks, () => SelectedPriceType.Value = DownloadPriceChoices.Both);
        }
        
        public SymbolSetupType SetupType { get; private set; }
        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<DownloadPriceChoices> AvailablePriceTypes => EnumHelper.AllValues<DownloadPriceChoices>();
        public Property<List<TimeFrames>> AvailableBases { get; }
        public IObservableList<SymbolData> AvailableSymbols { get; }
        public Property<SymbolData> SelectedSymbol { get; }
        public Property<TimeFrames> SelectedTimeframe { get; }
        public Property<DownloadPriceChoices> SelectedPriceType { get; }
        public Property<Tuple<DateTime, DateTime>> AvailableRange { get; }
        public BoolVar IsUpdating { get; }
        public BoolVar CanChangePrice { get; }

        public void Add() => OnAdd?.Invoke();
        public void Remove() => Removed?.Invoke(this);

        public event System.Action<BacktesterSymbolSetupViewModel> Removed;
        public event System.Action OnAdd;

        private async void UpdateAvailableRange()
        {
            var smb = SelectedSymbol.Value;

            if (smb != null)
            {
                _requestsCount.Value++;

                try
                {
                    AvailableRange.Value = await smb.GetAvailableRange(TimeFrames.M1, BarPriceType.Bid);
                }
                catch (Exception ex)
                {
                    //TO DO
                }

                _requestsCount.Value--;
            }
        }

        public async Task PrecacheData(IActionObserver observer, CancellationToken cToken, DateTime fromLimit, DateTime toLimit)
        {
            if (SetupType == SymbolSetupType.MainSymbol)
                return;

            var precacheFrom = GetLocalFrom(fromLimit);
            var precacheTo = GetLocalTo(toLimit);

            if (SetupType != SymbolSetupType.MainSymbol)
            {
                var smb = SelectedSymbol.Value;
                var priceChoice = SelectedPriceType.Value;
                var timeFrameChoice = SelectedTimeframe.Value;

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
        }

        public void Apply(Backtester tester, DateTime fromLimit, DateTime toLimit)
        {
            var smbData = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;
            var timeframe = SelectedTimeframe.Value;

            if (SetupType == SymbolSetupType.MainSymbol)
            {
                tester.MainSymbol = smbData.Name;
                tester.MainTimeframe = timeframe;
            }
            else
            {
                var precacheFrom = GetLocalFrom(fromLimit);
                var precacheTo = GetLocalTo(toLimit);

                tester.Symbols.Add(smbData.Name, smbData.InfoEntity);

                if (timeframe == TimeFrames.Ticks || timeframe == TimeFrames.TicksLevel2)
                {
                    ITickStorage feed = smbData.GetCrossDomainTickReader(timeframe, precacheFrom, precacheTo);

                    tester.Feed.AddSource(smbData.Name, feed);
                }
                else
                {
                    IBarStorage bidFeed = null;
                    IBarStorage askFeed = null;

                    if (priceChoice == DownloadPriceChoices.Bid | priceChoice == DownloadPriceChoices.Both)
                        bidFeed = smbData.GetCrossDomainBarReader(timeframe, BarPriceType.Bid, precacheFrom, precacheTo);

                    if (priceChoice == DownloadPriceChoices.Ask | priceChoice == DownloadPriceChoices.Both)
                        askFeed = smbData.GetCrossDomainBarReader(timeframe, BarPriceType.Ask, precacheFrom, precacheTo);

                    tester.Feed.AddSource(smbData.Name, timeframe, bidFeed, askFeed);
                }
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
    internal enum SymbolSetupType { MainSymbol, MainFeed, AdditionalFeed }
}
