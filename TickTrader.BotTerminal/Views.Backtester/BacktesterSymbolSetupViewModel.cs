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

            if (type == SymbolSetupType.Main)
                AvailableTimeFrames = EnumHelper.AllValues<TimeFrames>().Where(t => !t.IsTicks());
            else
                AvailableTimeFrames = EnumHelper.AllValues<TimeFrames>();

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

            TriggerOn(isTicks, () => SelectedPriceType.Value = DownloadPriceChoices.Both);

            SelectDefaultSymbol();
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
        public BoolVar IsSymbolSelected { get; }

        public void Add() => OnAdd?.Invoke();
        public void Remove() => Removed?.Invoke(this);

        public event System.Action<BacktesterSymbolSetupViewModel> Removed;
        public event System.Action OnAdd;

        public string AsText()
        {
            return SelectedSymbol.Value.Name + " " + SelectedTimeframe.Value;
        }

        public async void UpdateAvailableRange(TimeFrames timeFrame)
        {
            var smb = SelectedSymbol.Value;

            if (smb != null)
            {
                _requestsCount.Value++;

                try
                {
                    //AvailableRange.Value = await smb.GetAvailableRange(SelectedTimeframe.Value, BarPriceType.Bid);
                    var range = await smb.GetAvailableRange(timeFrame, BarPriceType.Bid);
                    AvailableRange.Value = new Tuple<DateTime, DateTime>(range.Item1.Date, range.Item2.Date + TimeSpan.FromDays(1));
                }
                catch (Exception ex)
                {
                    //TO DO
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

        public void Apply(Backtester tester, DateTime fromLimit, DateTime toLimit)
        {
            Apply(tester, fromLimit, toLimit, SelectedTimeframe.Value);
        }

        public void Apply(Backtester tester, DateTime fromLimit, DateTime toLimit, TimeFrames baseTimeFrame)
        {
            var smbData = SelectedSymbol.Value;
            var priceChoice = SelectedPriceType.Value;

            if (smbData == null)
                return;

            if (SetupType == SymbolSetupType.Main)
            {
                tester.MainSymbol = smbData.Name;
                tester.MainTimeframe = SelectedTimeframe.Value; // SelectedTimeframe may differ from baseTimeFrame in case of main symbol
            }

            var precacheFrom = GetLocalFrom(fromLimit);
            var precacheTo = GetLocalTo(toLimit);

            tester.Symbols.Add(smbData.Name, smbData.InfoEntity);

            if (baseTimeFrame == TimeFrames.Ticks || baseTimeFrame == TimeFrames.TicksLevel2)
            {
                ITickStorage feed = smbData.GetCrossDomainTickReader(baseTimeFrame, precacheFrom, precacheTo);

                tester.Feed.AddSource(smbData.Name, feed);
            }
            else
            {
                IBarStorage bidFeed = null;
                IBarStorage askFeed = null;

                if (priceChoice == DownloadPriceChoices.Bid | priceChoice == DownloadPriceChoices.Both)
                    bidFeed = smbData.GetCrossDomainBarReader(baseTimeFrame, BarPriceType.Bid, precacheFrom, precacheTo);

                if (priceChoice == DownloadPriceChoices.Ask | priceChoice == DownloadPriceChoices.Both)
                    askFeed = smbData.GetCrossDomainBarReader(baseTimeFrame, BarPriceType.Ask, precacheFrom, precacheTo);

                tester.Feed.AddSource(smbData.Name, baseTimeFrame, bidFeed, askFeed);
            }
        }

        public void Reset()
        {
            if (SelectedSymbol.Value == null || !AvailableSymbols.Contains(SelectedSymbol.Value))
                SelectDefaultSymbol();
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
            base.Dispose();
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
