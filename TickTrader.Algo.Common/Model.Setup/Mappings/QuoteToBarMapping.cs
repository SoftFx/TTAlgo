using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarMapping : SymbolMapping
    {
        private QuoteToBarReduction _reduction;


        internal QuoteToBarMapping() : this("Bid", new QuoteToBidBarReduction())
        {
        }

        internal QuoteToBarMapping(string name, QuoteToBarReduction reduction)
            : base(name)
        {
            _reduction = reduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Bar>(inputName, symbol, MapValue);
        }


        private BarEntity MapValue(QuoteEntity quote)
        {
            var res = new BarEntity
            {
                OpenTime = quote.Time,
                CloseTime = quote.Time,
            };
            _reduction.Reduce(quote, res);
            return res;
        }
    }
}
