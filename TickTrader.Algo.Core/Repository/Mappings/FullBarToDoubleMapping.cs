using System;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    internal class FullBarToDoubleMapping
    {
        private readonly FullBarToBarReduction _barReductionInstance;
        private readonly BarToDoubleReduction _doubleReductionInstance;
        private readonly FullBarToDoubleReduction _fullDoubleReductionInstance;


        public Func<BarData, BarData, double> MapValue { get; }


        public FullBarToDoubleMapping(ReductionKey barReductionKey, ReductionKey doubleReductionKey)
        {
            if (doubleReductionKey == null)
            {
                var fullDoubleReduction = AlgoAssemblyInspector.GetReduction(barReductionKey.DescriptorId);
                _fullDoubleReductionInstance = fullDoubleReduction.CreateInstance<FullBarToDoubleReduction>();
                MapValue = MapValueStraight;
            }
            else
            {
                var barReduction = AlgoAssemblyInspector.GetReduction(barReductionKey.DescriptorId);
                var doubleReduction = AlgoAssemblyInspector.GetReduction(doubleReductionKey.DescriptorId);
                _barReductionInstance = barReduction.CreateInstance<FullBarToBarReduction>();
                _doubleReductionInstance = doubleReduction.CreateInstance<BarToDoubleReduction>();
                MapValue = MapValueComposite;
            }
        }


        private double MapValueStraight(BarData bidBar, BarData askBar)
        {
            return _fullDoubleReductionInstance.Reduce(new BarEntity(bidBar), new BarEntity(askBar));
        }

        private double MapValueComposite(BarData bidBar, BarData askBar)
        {
            var bar = new BarEntity(BarData.CreateBlank(bidBar.OpenTime, bidBar.CloseTime));
            _barReductionInstance.Reduce(new BarEntity(bidBar), new BarEntity(askBar), bar);
            return _doubleReductionInstance.Reduce(bar);
        }
    }
}
