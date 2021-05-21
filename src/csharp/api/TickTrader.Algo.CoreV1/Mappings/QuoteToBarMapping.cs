using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class QuoteToBarMapping
    {
        private readonly QuoteToBarReduction _barReductionInstance;


        public QuoteToBarMapping(ReductionKey barReductionKey)
        {
            var barReduction = PackageMetadataCache.GetReduction(barReductionKey);
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
