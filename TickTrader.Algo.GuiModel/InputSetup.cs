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
    }

    public abstract class BarInputSetup : InputSetup
    {
        public abstract void Configure(DirectReader<Bar> reader);

        public class Invalid : BarInputSetup
        {
            public Invalid(InputInfo descriptor, LocMsg error)
            {
                SetMetadata(descriptor);
                this.Error = error;
            }

            public override void Configure(DirectReader<Bar> reader)
            {
                throw new Exception("Cannot configure invalid input!");
            }
        }

        public class BarToDouble : BarInputSetup
        {
            public BarToDouble(InputInfo descriptor)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(DirectReader<Bar> reader)
            {
                reader.AddMapping(Id, b => b.High);
            }
        }

        public class BarToBar : BarInputSetup
        {
            public BarToBar(InputInfo descriptor)
            {
                SetMetadata(descriptor);
            }

            public override void Configure(DirectReader<Bar> reader)
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
