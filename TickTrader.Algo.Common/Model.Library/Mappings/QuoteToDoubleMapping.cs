using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Library
{
    public class QuoteToDoubleMapping : Mapping
    {
        private ReductionMetadata _barReduction;
        private ReductionMetadata _doubleReduction;


        internal QuoteToDoubleMapping()
        {
            Key = new MappingKey(MappingCollection.DefaultQuoteToDoubleReduction);
            DisplayName = "Bid";
        }

        internal QuoteToDoubleMapping(ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
            : base(doubleReductionKey, doubleReduction)
        {
            _doubleReduction = doubleReduction;
        }

        internal QuoteToDoubleMapping(ReductionKey barReductionKey, ReductionMetadata barReduction, ReductionKey doubleReductionKey, ReductionMetadata doubleReduction)
            : base(barReductionKey, barReduction, doubleReductionKey, doubleReduction)
        {
            _barReduction = barReduction;
            _doubleReduction = doubleReduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            if (_barReduction == null)
            {
                var doubleReductionInstance = _doubleReduction?.CreateInstance<QuoteToDoubleReduction>() ?? new QuoteToBestBidReduction();
                target.GetFeedStrategy<QuoteStrategy>().MapInput(inputName, symbol, q => MapValueStraight(doubleReductionInstance, q));
            }
            else
            {
                var barReductionInstance = _barReduction.CreateInstance<QuoteToBarReduction>();
                var doubleReductionInstance = _doubleReduction.CreateInstance<BarToDoubleReduction>();
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
