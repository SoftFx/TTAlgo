using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedDownloadViewModel : BaseLoadingWindow
    {
        private readonly ISymbolCatalog _catalog;


        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames { get; } = TimeFrameModel.AllTimeFrames;

        public IEnumerable<Feed.Types.MarketSide> AvailablePriceTypes { get; } = EnumHelper.AllValues<Feed.Types.MarketSide>();

        public IEnumerable<ISymbolData> Symbols { get; }


        public Property<ISymbolData> SelectedSymbol { get; }

        public Property<Feed.Types.Timeframe> SelectedTimeFrame { get; }

        public Property<Feed.Types.MarketSide> SelectedPriceType { get; }


        public BoolVar DownloadEnabled { get; }

        public BoolVar IsSelectedTick { get; }


        public FeedDownloadViewModel(ISymbolCatalog catalog, ISymbolData symbol) : base("Pre-download symbol")
        {
            _catalog = catalog;

            Symbols = catalog.AllSymbols;

            SelectedTimeFrame = _varContext.AddProperty(Feed.Types.Timeframe.M1);
            SelectedPriceType = _varContext.AddProperty(Feed.Types.MarketSide.Bid);
            SelectedSymbol = _varContext.AddProperty(symbol);

            IsSelectedTick = !SelectedTimeFrame.Var.IsTicks();
            DownloadEnabled = (SelectedSymbol.Value?.IsDownloadAvailable ?? false) & IsReadyProgress;

            _varContext.TriggerOnChange(SelectedSymbol.Var, UpdateAvailableRange);
        }


        public void Download()
        {
            ShowProgressUi.Value = true;
            ProgressObserver.Start(DownloadAsync);
        }


        private async void UpdateAvailableRange(VarChangeEventArgs<ISymbolData> args)
        {
            if (args.New != null)
            {
                var range = await args.New.GetAvailableRange(Feed.Types.Timeframe.M1);
                UpdateAvailableRange(range);
            }
        }

        private async Task DownloadAsync(IActionObserver observer)
        {
            var timeFrame = SelectedTimeFrame.Value;
            var from = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(DateRange.To + TimeSpan.FromDays(1), DateTimeKind.Utc);

            observer?.SetMessage("Downloading... \n");

            if (timeFrame.IsTicks())
                await SelectedSymbol.Value.DownloadTicksWithObserver(observer, timeFrame, from, to);
            else
                await SelectedSymbol.Value.DownloadBarWithObserver(observer, timeFrame, SelectedPriceType.Value, from, to);
        }
    }
}
