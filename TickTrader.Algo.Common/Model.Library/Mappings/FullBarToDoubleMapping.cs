using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public class FullBarToDoubleMapping : Mapping
    {
        private ReductionMetadata _barReduction;
        private ReductionMetadata _doubleReduction;


        internal FullBarToDoubleMapping(ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
            : base(doubleReductionKey, doubleReduction)
        {
            _doubleReduction = doubleReduction;
        }

        internal FullBarToDoubleMapping(ReductionKey barReductionKey, ReductionMetadata barReduction, ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
            : base(barReductionKey, barReduction, doubleReductionKey, doubleReduction)
        {
            _barReduction = barReduction;
            _doubleReduction = doubleReduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            if (_barReduction == null)
            {
                var doubleReductionInstance = _doubleReduction.CreateInstance<FullBarToDoubleReduction>();
                target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, (bidBar, askBar) => MapValueStraight(doubleReductionInstance, bidBar, askBar));
            }
            else
            {
                var barReductionInstance = _barReduction.CreateInstance<FullBarToBarReduction>();
                var doubleReductionInstance = _doubleReduction.CreateInstance<BarToDoubleReduction>();
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
