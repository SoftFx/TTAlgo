using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class AskBarMapping : SymbolMapping
    {
        internal AskBarMapping() : base("Ask")
        {
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Ask);
        }
    }


    public class AskBarToDoubleMapping : SymbolMapping
    {
        private BarToDoubleReduction _reduction;


        internal AskBarToDoubleMapping() : this("Close", new BarToCloseReduction())
        {
        }

        internal AskBarToDoubleMapping(string name, BarToDoubleReduction reduction)
            : base($"Ask.{name}")
        {
            _reduction = reduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Ask, MapValue);
        }


        private double MapValue(BarEntity bar)
        {
            return _reduction.Reduce(bar);
        }
    }
}
