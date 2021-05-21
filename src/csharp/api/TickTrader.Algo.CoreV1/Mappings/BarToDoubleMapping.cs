using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class BarToDoubleMapping
    {
        private readonly BarToDoubleReduction _doubleReductionInstance;


        public BarToDoubleMapping(ReductionKey doubleReductionKey)
        {
            var doubleReduction = PackageMetadataCache.GetReduction(doubleReductionKey);
            _doubleReductionInstance = doubleReduction.CreateInstance<BarToDoubleReduction>();
        }


        public double MapValue(BarData bar)
        {
            return _doubleReductionInstance.Reduce(new BarEntity(bar));
        }
    }
}
