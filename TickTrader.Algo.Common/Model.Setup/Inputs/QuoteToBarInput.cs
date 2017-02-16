using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarInput : SingleBarInputBase
    {
        private static readonly Func<QuoteEntity, Bar> askConvertor = q =>
            new BarEntity()
            {
                Open = q.Ask,
                Close = q.Ask,
                High = q.Ask,
                Low = q.Ask,
                OpenTime = q.Time,
                CloseTime = q.Time,
                Volume = 1
            };

        private static readonly Func<QuoteEntity, Bar> bidConvertor = q =>
            new BarEntity()
            {
                Open = q.Bid,
                Close = q.Bid,
                High = q.Bid,
                Low = q.Bid,
                OpenTime = q.Time,
                CloseTime = q.Time,
                Volume = 1
            };

        public QuoteToBarInput(InputDescriptor descriptor, string symbolCode, BarPriceType defPriceType)
            : base(descriptor, symbolCode, defPriceType)
        {
            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Bar>(Descriptor.Id, SelectedSymbol.Name,
                PriceType == BarPriceType.Ask ? askConvertor : bidConvertor);
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as QuoteToBarInput;
            SelectedSymbol = otherInput.SelectedSymbol;
            PriceType = otherInput.PriceType;
        }
    }
}
