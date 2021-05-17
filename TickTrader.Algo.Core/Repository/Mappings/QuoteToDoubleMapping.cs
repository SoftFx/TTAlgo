using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    internal class QuoteToDoubleMapping
    {
        private readonly QuoteToDoubleReduction _doubleReductionInstance;


        public QuoteToDoubleMapping(ReductionKey doubleReductionKey)
        {
            var doubleReduction = AlgoAssemblyInspector.GetReduction(doubleReductionKey.DescriptorId);
            _doubleReductionInstance = doubleReduction.CreateInstance<QuoteToDoubleReduction>();
        }


        public double MapValue(QuoteInfo quote)
        {
            return _doubleReductionInstance.Reduce(new QuoteEntity(quote));
        }
    }
}
