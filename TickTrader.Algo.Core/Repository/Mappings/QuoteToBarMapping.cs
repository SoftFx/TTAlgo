using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    internal class QuoteToBarMapping
    {
        private readonly QuoteToBarReduction _barReductionInstance;


        public QuoteToBarMapping(ReductionKey barReductionKey)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(barReductionKey.DescriptorId);
            _barReductionInstance = barReduction.CreateInstance<QuoteToBarReduction>();
        }


        public BarEntity MapValue(QuoteInfo quote)
        {
            var res = new BarEntity(BarData.CreateBlank(quote.Timestamp, quote.Timestamp));
            _barReductionInstance.Reduce(new QuoteEntity(quote), res);
            return res;
        }
    }
}
