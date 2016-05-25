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
    public class IndicatorSetup_Bars : IndicatorSetupBase, IPluginSetup
    {
        private string mainSymbol;

        public IndicatorSetup_Bars(AlgoPluginDescriptor descriptor, string mainSymbol)
            : base(descriptor)
        {
            this.mainSymbol = mainSymbol;

            Init();
        }

        public PluginBuilder CreateBuilder()
        {
            var builder = new IndicatorBuilder(Descriptor);

            foreach (var input in Inputs)
                ((BarInputSetup)input).Configure(builder);

            foreach (var parameter in Parameters)
                builder.SetParameter(parameter.Id, parameter.ValueObj);

            return builder;
        }

        protected override InputSetup CreateInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new BarInputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInput(descriptor, mainSymbol);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInput(descriptor, mainSymbol);
                default: return new BarInputSetup.Invalid(descriptor, "UnsupportedInputType");
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
