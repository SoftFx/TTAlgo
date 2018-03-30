using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class QuoteToDoubleMapping : SymbolMapping
    {
        private QuoteToDoubleReduction _straightReduction;
        private QuoteToBarReduction _compositeReductionBar;
        private BarToDoubleReduction _compositeReductionDouble;


        internal QuoteToDoubleMapping() : this("BestBid", new QuoteToBestBidReduction())
        {
        }

        internal QuoteToDoubleMapping(string name, QuoteToDoubleReduction reduction)
            : base(name)
        {
            _straightReduction = reduction;
        }

        internal QuoteToDoubleMapping(string name, QuoteToBarReduction reductionBar)
            : this(name, reductionBar, "Close", new BarToCloseReduction())
        {
        }

        internal QuoteToDoubleMapping(string nameBar, QuoteToBarReduction reductionBar,
            string nameDouble, BarToDoubleReduction reductionDouble) : base($"{nameBar}.{nameDouble}")
        {
            _compositeReductionBar = reductionBar;
            _compositeReductionDouble = reductionDouble;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            if (_straightReduction != null)
            {
                target.GetFeedStrategy<QuoteStrategy>().MapInput(inputName, symbol, MapValueStraight);
            }
            else
            {
                target.GetFeedStrategy<QuoteStrategy>().MapInput(inputName, symbol, MapValueComposite);
            }
        }


        private double MapValueStraight(QuoteEntity quote)
        {
            return _straightReduction.Reduce(quote);
        }

        private double MapValueComposite(QuoteEntity quote)
        {
            var bar = new BarEntity();
            _compositeReductionBar.Reduce(quote, bar);
            return _compositeReductionDouble.Reduce(bar);
        }
    }
}
