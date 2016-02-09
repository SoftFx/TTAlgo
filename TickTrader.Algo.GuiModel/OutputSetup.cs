using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class OutputSetup : PropertySetupBase
    {
        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }

        public OutputSetup(OutputInfo descriptor)
        {
            SetMetadata(descriptor);
        }
    }

    public class NotSupportedOuput : OutputSetup
    {
        public NotSupportedOuput(OutputInfo descriptor)
            : base(descriptor)
        {
            SetMetadata(descriptor);
        }
    }

    public class ColoredLineOutputSetup : OutputSetup
    {
        public ColoredLineOutputSetup(OutputInfo descriptor)
            : base(descriptor)
        {
            SetMetadata(descriptor);
        }
    }
}
