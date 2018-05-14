using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Library
{
    public class QuoteToBarMapping : Mapping
    {
        private ReductionMetadata _barReduction;


        internal QuoteToBarMapping()
        {
            Key = new MappingKey(new ReductionKey("TickTrader.Algo.Common.dll", RepositoryLocation.Embedded, nameof(QuoteToBidBarReduction)));
            DisplayName = "Bid";
        }

        internal QuoteToBarMapping(ReductionKey barReductionKey, ReductionMetadata barReduction)
            : base(barReductionKey, barReduction)
        {
            _barReduction = barReduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var barReductionInstance = _barReduction?.CreateInstance<QuoteToBarReduction>() ?? new QuoteToBidBarReduction();
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
