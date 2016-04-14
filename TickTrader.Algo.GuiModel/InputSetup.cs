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
        public BarInputSetup(InputDescriptor descriptor, string symbolCode)
        {
            SetMetadata(descriptor);
            SymbolCode = symbolCode;
        }

        public string SymbolCode { get; private set; }
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

        public class BarToDouble : BarInputSetup
        {
            public BarToDouble(InputDescriptor descriptor, string symbolCode)
                : base(descriptor, symbolCode)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(IndicatorBuilder builder)
            {
                builder.MapBarInput(Descriptor.Id, SymbolCode, b => b.High);
            }
        }

        public class BarToBar : BarInputSetup
        {
            public BarToBar(InputDescriptor descriptor, string symbolCode)
                : base(descriptor, symbolCode)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(IndicatorBuilder builder)
            {
                builder.MapBarInput(Descriptor.Id, SymbolCode);
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
