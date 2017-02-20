using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToQuoteInput : InputSetup
    {
        private bool useL2;

        public QuoteToQuoteInput(InputDescriptor descriptor, string symbolCode, bool useL2)
            : base(descriptor, symbolCode, null)
        {
            this.useL2 = useL2;

            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            //if (useL2)
            //    target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
            //else
                target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Quote>(Descriptor.Id, SelectedSymbol.Name, b => b);
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToBarInput;
            SelectedSymbol = otherInput.SelectedSymbol;
        }
    }

    //public class QuoteToQuoteL2Input : InputSetup
    //{
    //    public QuoteToQuoteL2Input(InputDescriptor descriptor, string symbolCode)
    //        : base(descriptor, symbolCode)
    //    {
    //        SetMetadata(descriptor);
    //    }

    //    public override void Apply(IPluginSetupTarget target)
    //    {
    //        target.MapInput<QuoteEntityL2, Api.QuoteL2>(Descriptor.Id, SymbolCode, b => b);
    //    }

    //    public override void Configure(IndicatorBuilder builder)
    //    {
    //        builder.MapBarInput(Descriptor.Id, SymbolCode);
    //    }

    //    public override void CopyFrom(PropertySetupBase srcProperty)
    //    {
    //        var otherInput = srcProperty as BarToBarInput;
    //        SymbolCode = otherInput.SymbolCode;
    //    }
    //}

    public class QuoteToDoubleInput : InputSetup
    {
        private QuoteToDoubleMappings mapping;

        public QuoteToDoubleInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode, null)
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
            target.GetFeedStrategy<QuoteStrategy>(). MapInput<double>(Descriptor.Id, SelectedSymbol.Name, GetSelector());
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
            SelectedSymbol = otherInput.SelectedSymbol;
            Mapping = otherInput.Mapping;
        }
    }
}
