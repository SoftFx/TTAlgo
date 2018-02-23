using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class FullBarToDoubleMapping : SymbolMapping
    {
        private FullBarToDoubleReduction _straightReduction;
        private FullBarToBarReduction _compositeReductionBar;
        private BarToDoubleReduction _compositeReductionDouble;


        internal FullBarToDoubleMapping() : this("Bid.Close", new FullBarToBidCloseReduction())
        {
        }

        internal FullBarToDoubleMapping(string name, FullBarToDoubleReduction reduction)
            : base(name)
        {
            _straightReduction = reduction;
        }

        internal FullBarToDoubleMapping(string name, FullBarToBarReduction reductionBar)
            : this(name, reductionBar, "Close", new BarToCloseReduction())
        {
        }

        internal FullBarToDoubleMapping(string nameBar, FullBarToBarReduction reductionBar,
            string nameDouble, BarToDoubleReduction reductionDouble) : base($"{nameBar}.{nameDouble}")
        {
            _compositeReductionBar = reductionBar;
            _compositeReductionDouble = reductionDouble;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            if (_straightReduction != null)
            {
                target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, MapValueStraight);
            }
            else
            {
                target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, MapValueComposite);
            }
        }


        private double MapValueStraight(BarEntity bidBar, BarEntity askBar)
        {
            return _straightReduction.Reduce(bidBar, askBar);
        }

        private double MapValueComposite(BarEntity bidBar, BarEntity askBar)
        {
            var bar = new BarEntity();
            _compositeReductionBar.Reduce(bidBar, askBar, bar);
            return _compositeReductionDouble.Reduce(bar);
        }
    }
}
