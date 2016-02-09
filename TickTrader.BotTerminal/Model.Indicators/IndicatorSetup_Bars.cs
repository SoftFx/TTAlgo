using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    public class IndicatorSetup_Bars : IndicatorSetupBase
    {
        public IndicatorSetup_Bars(AlgoInfo descriptor)
            : base(descriptor)
        {
        }

        protected override InputSetup CreateInput(InputInfo descriptor)
        {
            if (!descriptor.IsValid)
                return new BarInputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarInputSetup.BarToDouble(descriptor);
                case "TickTrader.Algo.Api": return new BarInputSetup.BarToBar(descriptor);
                default: return new BarInputSetup.Invalid(descriptor, "UnsupportedInputType");
            }
        }

        protected override OutputSetup CreateOuput(OutputInfo descriptor)
        {
            if (descriptor.DataSeriesBaseTypeFullName == "System.Double")
                return new ColoredLineOutputSetup(descriptor);
            else
                return new NotSupportedOuput(descriptor);
        }
    }
}
