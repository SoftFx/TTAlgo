using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;

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

        private BarEntity MapValue(QuoteToBarReduction reductionInstance, QuoteEntity quote)
        {
            var res = new BarEntity
            {
                OpenTime = quote.Time,
                CloseTime = quote.Time,
            };
            reductionInstance.Reduce(quote, res);
            return res;
        }
    }
}
