using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    internal class FullBarToBarMapping
    {
        private readonly FullBarToBarReduction _barReductionInstance;


        public FullBarToBarMapping(ReductionKey barReductionKey)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(barReductionKey.DescriptorId);
            _barReductionInstance = barReduction.CreateInstance<FullBarToBarReduction>();
        }


        public BarEntity MapValue(BarData bidBar, BarData askBar)
        {
            var res = new BarEntity(BarData.CreateBlank(bidBar.OpenTime, bidBar.CloseTime));
            _barReductionInstance.Reduce(new BarEntity(bidBar), new BarEntity(askBar), res);
            return res;
        }
    }
}
