using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarInputSetup : SingleBarInputSetupBase
    {
        private static readonly Func<QuoteEntity, Bar> AskConvertor = q =>
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

        private static readonly Func<QuoteEntity, Bar> BidConvertor = q =>
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


        public QuoteToBarInputSetup(InputDescriptor descriptor, string symbolCode, BarPriceType defPriceType)
            : base(descriptor, symbolCode, defPriceType)
        {
            SetMetadata(descriptor);
        }


        public override void Apply(IPluginSetupTarget target)
        {
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Bar>(Descriptor.Id, SelectedSymbol.Name,
                PriceType == BarPriceType.Ask ? AskConvertor : BidConvertor);
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToBarInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToBarInput();
            SaveConfig(input);
            return input;
        }
    }
}
