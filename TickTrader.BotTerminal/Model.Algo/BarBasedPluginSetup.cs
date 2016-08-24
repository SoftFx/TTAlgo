using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Realtime;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    public class BarBasedPluginSetup : PluginSetup, IPluginSetup
    {
        private string mainSymbol;

        public BarBasedPluginSetup(AlgoPluginRef pRef, string mainSymbol)
            : base(pRef)
        {
            this.mainSymbol = mainSymbol;

            Init();
        }

        public PluginBuilder CreateBuilder()
        {
            var builder = new IndicatorBuilder(Descriptor);

            foreach (var input in Inputs)
                input.Configure(builder);

            foreach (var parameter in Parameters)
                builder.SetParameter(parameter.Id, parameter.ValueObj);

            return builder;
        }

        protected override InputSetup CreateInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInput(descriptor, mainSymbol);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInput(descriptor, mainSymbol);
                //case "TickTrader.Algo.Api.Quote": return new QuoteToQuoteInput(descriptor, mainSymbol, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteToQuoteInput(descriptor, mainSymbol, true);
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
                return new ColoredLineOutputSetup(descriptor, Algo.GuiModel.MsgCodes.UnsupportedPropertyType);
        }
    }
}
