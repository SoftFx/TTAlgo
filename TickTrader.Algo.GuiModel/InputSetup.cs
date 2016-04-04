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
        public abstract void Configure(DirectReader<Api.Bar> reader);

        public class Invalid : BarInputSetup
        {
            public Invalid(InputDescriptor descriptor, object error = null)
            {
                SetMetadata(descriptor);
                if (error == null)
                    this.Error = new GuiModelMsg(descriptor.Error.Value);
                else
                    this.Error = new GuiModelMsg(error);
            }

            public Invalid(InputDescriptor descriptor, GuiModelMsg error)
            {
                SetMetadata(descriptor);
                this.Error = error;
            }

            public override void Configure(DirectReader<Api.Bar> reader)
            {
                throw new Exception("Cannot configure invalid input!");
            }
        }

        public class BarToDouble : BarInputSetup
        {
            public BarToDouble(InputDescriptor descriptor)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(DirectReader<Api.Bar> reader)
            {
                reader.AddMapping(Id, b => b.High);
            }
        }

        public class BarToBar : BarInputSetup
        {
            public BarToBar(InputDescriptor descriptor)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(DirectReader<Api.Bar> reader)
            {
                reader.AddMapping(Id, b => b);
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
