using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public class AskBarMapping : Mapping
    {
        internal AskBarMapping()
        {
            Key = new MappingKey(MappingCollection.AskBarReduction);
            DisplayName = "Ask";
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Ask);
        }
    }


    public class AskBarToDoubleMapping : Mapping
    {
        private ReductionMetadata _doubleReduction;


        internal AskBarToDoubleMapping()
        {
            Key = new MappingKey(MappingCollection.AskBarReduction, MappingCollection.DefaultBarToDoubleReduction);
            DisplayName = "Ask.Close";
        }

        internal AskBarToDoubleMapping(ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
        {
            _doubleReduction = doubleReduction;

            Key = new MappingKey(MappingCollection.AskBarReduction, doubleReductionKey);
            DisplayName = $"Ask.{_doubleReduction.Descriptor.DisplayName}";
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var doubleReductionInstance = _doubleReduction?.CreateInstance<BarToDoubleReduction>() ?? new BarToCloseReduction();
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Ask, bar => doubleReductionInstance.Reduce(bar));
        }
    }
}
