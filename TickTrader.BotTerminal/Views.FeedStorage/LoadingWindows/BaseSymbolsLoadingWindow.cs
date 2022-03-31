using Machinarium.Var;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal abstract class BaseSymbolsLoadingWindow : BaseLoadingWindow
    {
        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames { get; } = TimeFrameModel.AllTimeFrames;

        public IEnumerable<Feed.Types.MarketSide> AvailablePriceTypes { get; } = EnumHelper.AllValues<Feed.Types.MarketSide>();

        public IEnumerable<ISymbolData> Symbols { get; }


        public Property<ISymbolData> SelectedSymbol { get; }

        public Property<Feed.Types.Timeframe> SelectedTimeFrame { get; }

        public Property<Feed.Types.MarketSide> SelectedPriceType { get; }

        public BoolVar IsSelectedTick { get; }


        internal BaseSymbolsLoadingWindow(ISymbolCatalog catalog, ISymbolData symbol, string displayName) : base(displayName)
        {
            Symbols = catalog.AllSymbols;

            SelectedTimeFrame = _varContext.AddProperty(Feed.Types.Timeframe.M1);
            SelectedPriceType = _varContext.AddProperty(Feed.Types.MarketSide.Bid);
            SelectedSymbol = _varContext.AddProperty(symbol);

            IsSelectedTick = !SelectedTimeFrame.Var.IsTicks();
        }
    }
}
