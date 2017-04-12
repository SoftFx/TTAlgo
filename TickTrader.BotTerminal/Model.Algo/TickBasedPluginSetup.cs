using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class TickBasedPluginSetup : PluginSetup
    {
        private string mainSymbol;

        public TickBasedPluginSetup(AlgoPluginRef pRef, string mainSymbol)
            : base(pRef)
        {
            this.mainSymbol = mainSymbol;

            Init();
        }

        protected override InputSetup CreateInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInput(descriptor, mainSymbol);
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInput(descriptor, mainSymbol, Algo.Api.BarPriceType.Bid);
                case "TickTrader.Algo.Api.Quote": return new QuoteToQuoteInput(descriptor, mainSymbol, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteToQuoteInput(descriptor, mainSymbol, true);
                default: return new InputSetup.Invalid(descriptor, "UnsupportedInputType");
            }
        }

        protected override OutputSetup CreateOuput(OutputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new ErrorOutputSetup(descriptor);
            if (descriptor.DataSeriesBaseTypeFullName == "System.Double")
                return new ColoredLineOutputSetup(descriptor);
            else if (descriptor.DataSeriesBaseTypeFullName == "TickTrader.Algo.Api.Marker")
                return new MarkerSeriesOutputSetup(descriptor);
            else
                return new ColoredLineOutputSetup(descriptor, Algo.Common.Model.Setup.MsgCodes.UnsupportedPropertyType);
        }

        protected override PluginConfig SaveToConfig()
        {
            throw new NotImplementedException();
        }
    }
}
