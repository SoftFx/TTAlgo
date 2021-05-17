using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    internal class BarToDoubleMapping
    {
        private readonly BarToDoubleReduction _doubleReductionInstance;


        public BarToDoubleMapping(ReductionKey doubleReductionKey)
        {
            var doubleReduction = AlgoAssemblyInspector.GetReduction(doubleReductionKey.DescriptorId);
            _doubleReductionInstance = doubleReduction.CreateInstance<BarToDoubleReduction>();
        }


        public double MapValue(BarData bar)
        {
            return _doubleReductionInstance.Reduce(new BarEntity(bar));
        }
    }
}
