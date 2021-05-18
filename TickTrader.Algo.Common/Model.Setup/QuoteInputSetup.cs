using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToQuoteInputSetup : InputSetup
    {
        private bool _useL2;

        public QuoteToQuoteInputSetup(InputDescriptor descriptor, ISymbolInfo defaultSymbol, bool useL2)
            : base(descriptor, defaultSymbol, null)
        {
            _useL2 = useL2;

            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            //if (useL2)
            //    target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
            //else
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Quote>(Descriptor.Id, SelectedSymbol.Name, b => b);
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToQuoteInput;
            if (input != null)
            {
                _useL2 = input.UseL2;
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToQuoteInput { UseL2 = _useL2 };
            SaveConfig(input);
            return input;
        }
    }

    //public class QuoteToQuoteL2InputSetup : InputSetup
    //{
    //    public QuoteToQuoteL2InputSetup(InputDescriptor descriptor, string symbolCode)
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


    public enum QuoteToDoubleMappings { Ask, Bid, Median }

    public class QuoteToDoubleInputSetup : InputSetup
    {
        private QuoteToDoubleMappings _mapping;


        public QuoteToDoubleMappings Mapping
        {
            get { return _mapping; }
            set
            {
                _mapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<QuoteToDoubleMappings> AvailableMappings { get; private set; }


        public QuoteToDoubleInputSetup(InputDescriptor descriptor, ISymbolInfo defaultSymbol)
            : base(descriptor, defaultSymbol, null)
        {
            SetMetadata(descriptor);

            AvailableMappings = Enum.GetValues(typeof(QuoteToDoubleMappings)).Cast<QuoteToDoubleMappings>();
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.GetFeedStrategy<QuoteStrategy>().MapInput<double>(Descriptor.Id, SelectedSymbol.Name, GetSelector());
        }

        public override void Reset()
        {
            Mapping = QuoteToDoubleMappings.Ask;
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToDoubleInput;
            if (input != null)
            {
                _mapping = input.Mapping;
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToDoubleInput { Mapping = _mapping };
            SaveConfig(input);
            return input;
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
    }
}
