using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public class QuoteToQuoteInput : InputSetup
    {
        public QuoteToQuoteInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode);
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToBarInput;
            SymbolCode = otherInput.SymbolCode;
        }
    }

    public class QuoteToDoubleInput : InputSetup
    {
        private QuoteToDoubleMappings mapping;

        public QuoteToDoubleInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);

            this.AvailableMappings = Enum.GetValues(typeof(QuoteToDoubleMappings)).Cast<QuoteToDoubleMappings>();
        }

        public QuoteToDoubleMappings Mapping
        {
            get { return mapping; }
            set
            {
                this.mapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<QuoteToDoubleMappings> AvailableMappings { get; private set; }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput<QuoteEntity, double>(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapInput<QuoteEntity, double>(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Reset()
        {
            this.Mapping = QuoteToDoubleMappings.Ask;
        }

        private Func<QuoteEntity, double> GetSelector()
        {
            switch (Mapping)
            {
                case QuoteToDoubleMappings.Ask: return b => b.Ask;
                case QuoteToDoubleMappings.Bid: return b => b.Bid;
                case QuoteToDoubleMappings.Median: return b => (b.Ask + b.Bid) / 2;
                default: throw new Exception("Unknown mapping variant: " + Mapping);
            }
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as QuoteToDoubleInput;
            SymbolCode = otherInput.SymbolCode;
            Mapping = otherInput.Mapping;
        }
    }
}
