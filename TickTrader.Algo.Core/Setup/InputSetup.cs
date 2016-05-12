using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Realtime;
using TickTrader.Algo.Core.Setup.Serialization;

namespace TickTrader.Algo.Core.Setup
{
    public abstract class InputSetup : PropertySetupBase
    {
        public InputSetup(InputDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        public InputDescriptor Descriptor { get; private set; }
        public string SymbolCode { get; set; }
        public string MainSymbol { get; private set; }

        public override void Init()
        {
            SymbolCode = Metadata.MainSymbol;
        }

        protected override AlgoPropertyDescriptor GetDescriptor()
        {
            return Descriptor;
        }
    }

    public enum BarToDoubleMappings { Open, Close, High, Low, Mean }

    public class BarToDoubleInput : InputSetup
    {
        public BarToDoubleInput(InputDescriptor descriptor)
            : base(descriptor)
        {
        }

        public BarToDoubleMappings MappingVariant { get; set; }

        public override void Apply(PluginBuilder executor)
        {
            executor.MapBarInput<double>(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Deserialize(PropertySetup propObj)
        {
            var sameTypeInput = propObj as Serialization.BarToDoubleInput;
            if (sameTypeInput != null)
            {
                this.MappingVariant = sameTypeInput.Mapping;
                this.SymbolCode = NormalizeSymbol(sameTypeInput.Symbol);
            }
        }

        public override PropertySetup Serialize()
        {
            return new Serialization.BarToDoubleInput() { Name = Id, Symbol = SymbolCode, Mapping = MappingVariant };
        }

        private Func<BarEntity, double> GetSelector()
        {
            switch (MappingVariant)
            {
                case BarToDoubleMappings.Open: return b => b.Open;
                case BarToDoubleMappings.Close: return b => b.Close;
                case BarToDoubleMappings.High: return b => b.High;
                case BarToDoubleMappings.Low: return b => b.Low;
                case BarToDoubleMappings.Mean: return b => (b.High + b.Low) / 2;

                default: throw new Exception("Unknown mapping variant: " + MappingVariant);
            }
        }
    }

    public class BarInput : InputSetup
    {
        public BarInput(InputDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override void Apply(PluginBuilder executor)
        {
            executor.MapBarInput(Descriptor.Id, SymbolCode);
        }

        public override void Deserialize(PropertySetup propObj)
        {
            var sameTypeInput = propObj as Serialization.BarInput;
            if (sameTypeInput != null)
                this.SymbolCode = NormalizeSymbol(sameTypeInput.Symbol);
        }

        public override PropertySetup Serialize()
        {
            return new Serialization.BarInput() { Symbol = SymbolCode, Name = Id };
        }
    }

    //public enum QuoteToDoubleVariants { Ask, Bid, Mean }

    //[Serializable]
    //public class QuoteToDoubleInput : InputSetup
    //{
    //    public QuoteToDoubleInput(InputDescriptor descriptor)
    //        : base(descriptor)
    //    {
    //    }

    //    public QuoteToDoubleVariants MappingVariant { get; set; }

    //    public override void Apply(PluginExecutor executor)
    //    {
    //        executor.MapInput<QuoteEntity, double>(Descriptor.Id, SymbolCode, GetSelector());
    //    }

    //    public override void CopyFrom(PropertySetupBase srcProperty)
    //    {
    //        var sameTypeInput = srcProperty as QuoteToDoubleInput;
    //        if (sameTypeInput != null)
    //        {
    //            MappingVariant = sameTypeInput.MappingVariant;
    //            SymbolCode = NormalizeSymbol(sameTypeInput.SymbolCode);
    //        }
    //    }

    //    private Func<QuoteEntity, double> GetSelector()
    //    {
    //        switch (MappingVariant)
    //        {
    //            case QuoteToDoubleVariants.Ask: return q => q.Ask;
    //            case QuoteToDoubleVariants.Bid: return q => q.Bid;
    //            case QuoteToDoubleVariants.Mean: return q => (q.Ask + q.Bid) / 2;

    //            default: throw new Exception("Unknown mapping variant: " + MappingVariant);
    //        }
    //    }
    //}

    //[Serializable]
    //public class QuoteInput : InputSetup
    //{
    //    public QuoteInput(InputDescriptor descriptor)
    //        : base(descriptor)
    //    {
    //    }

    //    public override void Apply(PluginExecutor executor)
    //    {
    //        executor.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, q => q);
    //    }

    //    public override void CopyFrom(PropertySetupBase srcProperty)
    //    {
    //        var sameTypeInput = srcProperty as BarInput;
    //        if (sameTypeInput != null)
    //            SymbolCode = NormalizeSymbol(sameTypeInput.SymbolCode);
    //    }
    //}
}
