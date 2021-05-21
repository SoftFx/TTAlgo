using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class QuoteToDoubleMapping
    {
        private readonly QuoteToDoubleReduction _doubleReductionInstance;


        public QuoteToDoubleMapping(ReductionKey doubleReductionKey)
        {
            var doubleReduction = PackageMetadataCache.GetReduction(doubleReductionKey);
            _doubleReductionInstance = doubleReduction.CreateInstance<QuoteToDoubleReduction>();
        }


        public double MapValue(QuoteInfo quote)
        {
            return _doubleReductionInstance.Reduce(new QuoteEntity(quote));
        }
    }
}
