using System;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public class QuoteToDoubleMapping : Mapping
    {
        internal QuoteToDoubleMapping()
        {
            Key = new MappingKey(MappingCollection.DefaultQuoteToDoubleReduction);
            DisplayName = "Bid";
        }

        internal QuoteToDoubleMapping(ReductionKey barReductionKey, string barReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName)
        {
        }

        internal QuoteToDoubleMapping(ReductionKey barReductionKey, string barReductionDisplayName, ReductionKey doubleReductionKey, string doubleReductionDisplayName)
            : base(barReductionKey, barReductionDisplayName, doubleReductionKey, doubleReductionDisplayName)
        {
        }


        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReduction = AlgoAssemblyInspector.GetReduction(Key.PrimaryReduction.DescriptorId);
            var doubleReduction = AlgoAssemblyInspector.GetReduction(Key.SecondaryReduction.DescriptorId);
            if (doubleReduction == null)
            {
                var doubleReductionInstance = barReduction?.CreateInstance<QuoteToDoubleReduction>() ?? new QuoteToBestBidReduction();
                target.GetFeedStrategy<QuoteStrategy>().MapInput(inputName, symbol, q => MapValueStraight(doubleReductionInstance, q));
            }
            else
            {
                var barReductionInstance = barReduction.CreateInstance<QuoteToBarReduction>();
                var doubleReductionInstance = doubleReduction.CreateInstance<BarToDoubleReduction>();
                target.GetFeedStrategy<QuoteStrategy>().MapInput(inputName, symbol, q => MapValueComposite(barReductionInstance, doubleReductionInstance, q));
            }
        }


        private double MapValueStraight(QuoteToDoubleReduction reductionInstance, QuoteEntity quote)
        {
            return reductionInstance.Reduce(quote);
        }

        private double MapValueComposite(QuoteToBarReduction barReductionInstance, BarToDoubleReduction doubleReductionInstance, QuoteEntity quote)
        {
            var bar = new BarEntity
            {
                OpenTime = quote.Time,
                CloseTime = quote.Time,
            };
            barReductionInstance.Reduce(quote, bar);
            return doubleReductionInstance.Reduce(bar);
        }
    }
}
