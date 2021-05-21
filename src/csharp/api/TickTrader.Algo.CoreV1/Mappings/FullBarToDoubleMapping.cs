using System;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
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
                var fullDoubleReduction = PackageMetadataCache.GetReduction(barReductionKey);
                _fullDoubleReductionInstance = fullDoubleReduction.CreateInstance<FullBarToDoubleReduction>();
                MapValue = MapValueStraight;
            }
            else
            {
                var barReduction = PackageMetadataCache.GetReduction(barReductionKey);
                var doubleReduction = PackageMetadataCache.GetReduction(doubleReductionKey);
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
