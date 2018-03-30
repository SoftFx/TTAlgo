using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class BidBarMapping : SymbolMapping
    {
        internal BidBarMapping() : base("Bid")
        {
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid);
        }
    }


    public class BidBarToDoubleMapping : SymbolMapping
    {
        private BarToDoubleReduction _reduction;


        internal BidBarToDoubleMapping() : this("Close", new BarToCloseReduction())
        {
        }

        internal BidBarToDoubleMapping(string name, BarToDoubleReduction reduction)
            : base($"Bid.{name}")
        {
            _reduction = reduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid, MapValue);
        }


        private double MapValue(BarEntity bar)
        {
            return _reduction.Reduce(bar);
        }
    }
}
