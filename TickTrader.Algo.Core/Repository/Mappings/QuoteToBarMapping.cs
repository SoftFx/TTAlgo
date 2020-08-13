using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public class QuoteToBarMapping : Mapping
    {
        internal QuoteToBarMapping()
        {
            Key = new MappingKey(MappingCollection.DefaultQuoteToBarReduction);
            DisplayName = "Bid";
        }

        internal QuoteToBarMapping(ReductionKey barReductionKey, string barReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName)
        {
        }

        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(Key.PrimaryReduction.DescriptorId);
            var barReductionInstance = barReduction?.CreateInstance<QuoteToBarReduction>() ?? new QuoteToBidBarReduction();
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Bar>(inputName, symbol, q => MapValue(barReductionInstance, q));
        }

        private BarEntity MapValue(QuoteToBarReduction reductionInstance, QuoteInfo quote)
        {
            var res = new BarEntity(BarData.CreateBlank(quote.Timestamp, quote.Timestamp));
            reductionInstance.Reduce(new QuoteEntity(quote), res);
            return res;
        }
    }
}
