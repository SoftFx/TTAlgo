using System;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public class FullBarToDoubleMapping : Mapping
    {
        internal FullBarToDoubleMapping(ReductionKey barReductionKey, string barReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName)
        {
        }

        internal FullBarToDoubleMapping(ReductionKey barReductionKey, string barReductionDisplayName, ReductionKey doubleReductionKey, string doubleReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName, doubleReductionKey, doubleReductionDisplayName)
        {
        }


        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(Key.PrimaryReduction.DescriptorId);
            var doubleReduction = AlgoAssemblyInspector.GetReduction(Key.SecondaryReduction.DescriptorId);
            if (doubleReduction == null)
            {
                var doubleReductionInstance = barReduction.CreateInstance<FullBarToDoubleReduction>();
                target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, (bidBar, askBar) => MapValueStraight(doubleReductionInstance, bidBar, askBar));
            }
            else
            {
                var barReductionInstance = barReduction.CreateInstance<FullBarToBarReduction>();
                var doubleReductionInstance = doubleReduction.CreateInstance<BarToDoubleReduction>();
                target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, (bidBar, askBar) => MapValueComposite(barReductionInstance, doubleReductionInstance, bidBar, askBar));
            }
        }


        private double MapValueStraight(FullBarToDoubleReduction reductionInstance, BarEntity bidBar, BarEntity askBar)
        {
            return reductionInstance.Reduce(bidBar, askBar);
        }

        private double MapValueComposite(FullBarToBarReduction barReductionInstance, BarToDoubleReduction doubleReductionInstance, BarEntity bidBar, BarEntity askBar)
        {
            var bar = new BarEntity
            {
                OpenTime = bidBar.OpenTime,
                CloseTime = bidBar.CloseTime,
            };
            barReductionInstance.Reduce(bidBar, askBar, bar);
            return doubleReductionInstance.Reduce(bar);
        }
    }
}
