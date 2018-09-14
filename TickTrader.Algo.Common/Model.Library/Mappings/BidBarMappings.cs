using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public class BidBarMapping : Mapping
    {
        internal BidBarMapping()
        {
            Key = new MappingKey(MappingCollection.BidBarReduction);
            DisplayName = "Bid";
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid);
        }
    }


    public class BidBarToDoubleMapping : Mapping
    {
        private ReductionMetadata _doubleReduction;


        internal BidBarToDoubleMapping()
        {
            Key = new MappingKey(MappingCollection.BidBarReduction, MappingCollection.DefaultBarToDoubleReduction);
            DisplayName = "Bid.Close";
        }

        internal BidBarToDoubleMapping(ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
        {
            _doubleReduction = doubleReduction;

            Key = new MappingKey(MappingCollection.BidBarReduction, doubleReductionKey);
            DisplayName = $"Bid.{_doubleReduction.Descriptor.DisplayName}";
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var doubleReductionInstance = _doubleReduction?.CreateInstance<BarToDoubleReduction>() ?? new BarToCloseReduction();
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid, bar => doubleReductionInstance.Reduce(bar));
        }
    }
}
