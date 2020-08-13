using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public class FullBarToBarMapping : Mapping
    {
        internal FullBarToBarMapping(ReductionKey barReductionKey, string barReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName)
        {
        }

        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(Key.PrimaryReduction.DescriptorId);
            var barReductionInstance = barReduction.CreateInstance<FullBarToBarReduction>();
            target.GetFeedStrategy<BarStrategy>().MapInput<Bar>(inputName, symbol, (bidBar, askBar) => MapValue(barReductionInstance, bidBar, askBar));
        }

        private BarEntity MapValue(FullBarToBarReduction reductionInstance, BarData bidBar, BarData askBar)
        {
            var res = new BarEntity(BarData.CreateBlank(bidBar.OpenTime, bidBar.CloseTime));
            reductionInstance.Reduce(new BarEntity(bidBar), new BarEntity(askBar), res);
            return res;
        }
    }
}
