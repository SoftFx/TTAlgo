using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class InputSetup : PropertySetupBase
    {
        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }
    }

    public abstract class BarInputSetup : InputSetup
    {
        private string selectedSymbol;

        public BarInputSetup(InputDescriptor descriptor, string symbolCode)
        {
            SetMetadata(descriptor);
            SymbolCode = symbolCode;
            AvailableSymbols = new string[] { symbolCode };
            HasChoice = false;
        }

        public string SymbolCode
        {
            get { return selectedSymbol; }
            set
            {
                selectedSymbol = value;
                NotifyPropertyChanged(nameof(SymbolCode));
            }
        }

        public IEnumerable<string> AvailableSymbols { get; private set; }
        public bool HasChoice { get; private set; }

        public abstract void Configure(IndicatorBuilder builder);

        public class Invalid : BarInputSetup
        {
            public Invalid(InputDescriptor descriptor, object error = null)
                : base(descriptor, null)
            {
                if (error == null)
                    this.Error = new GuiModelMsg(descriptor.Error.Value);
                else
                    this.Error = new GuiModelMsg(error);
            }

            public Invalid(InputDescriptor descriptor, string symbol, GuiModelMsg error)
                : base(descriptor, symbol)
            {
                this.Error = error;
            }

            public override void Configure(IndicatorBuilder builder)
            {
                throw new Exception("Cannot configure invalid input!");
            }
        }

        public enum BarToDoubleMappings { Open, Close, High, Low, Mean }
    }

    public class BarToBarInput : BarInputSetup
    {
        public BarToBarInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode);
        }
    }

    public class BarToDoubleInput : BarInputSetup
    {
        private BarToDoubleMappings mapping;

        public BarToDoubleInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);

            this.AvailableMappings = Enum.GetValues(typeof(BarToDoubleMappings)).Cast<BarToDoubleMappings>();
        }

        public BarToDoubleMappings Mapping
        {
            get { return mapping; }
            set
            {
                this.mapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<BarToDoubleMappings> AvailableMappings { get; private set; }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Reset()
        {
            this.Mapping = BarToDoubleMappings.High;
        }

        private Func<BarEntity, double> GetSelector()
        {
            switch (Mapping)
            {
                case BarToDoubleMappings.Open: return b => b.Open;
                case BarToDoubleMappings.Close: return b => b.Close;
                case BarToDoubleMappings.High: return b => b.High;
                case BarToDoubleMappings.Low: return b => b.Low;
                case BarToDoubleMappings.Mean: return b => (b.High + b.Low) / 2;

                default: throw new Exception("Unknown mapping variant: " + Mapping);
            }
        }
    }

    //public class TickInputSetup : InputSetup
    //{
    //}

    //public class MultisymbolBarInputSetup : InputSetup
    //{
    //}
}
