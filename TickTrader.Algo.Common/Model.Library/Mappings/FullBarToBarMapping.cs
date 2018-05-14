using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public class FullBarToBarMapping : Mapping
    {
        private ReductionMetadata _barReduction;


        internal FullBarToBarMapping(ReductionKey barReductionKey, ReductionMetadata barReduction)
            : base(barReductionKey, barReduction)
        {
            _barReduction = barReduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReductionInstance = _barReduction.CreateInstance<FullBarToBarReduction>();
            target.GetFeedStrategy<BarStrategy>().MapInput<Bar>(inputName, symbol, (bidBar, askBar) => MapValue(barReductionInstance, bidBar, askBar));
        }


        private BarEntity MapValue(FullBarToBarReduction reductionInstance, BarEntity bidBar, BarEntity askBar)
        {
            var res = new BarEntity
            {
                OpenTime = bidBar.OpenTime,
                CloseTime = bidBar.CloseTime,
            };
            reductionInstance.Reduce(bidBar, askBar, res);
            return res;
        }
    }
}
