using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    [DataContract(Name = "BarBasedSetup", Namespace = "")]
    public class BarBasedPluginSetup : PluginSetup
    {
        [DataMember]
        private string mainSymbol;
        [DataMember]
        private BarPriceType priceType;
        private IAlgoGuiMetadata metadata;

        public BarBasedPluginSetup(AlgoPluginRef pRef, string mainSymbol, BarPriceType priceType, IAlgoGuiMetadata metadata)
            : base(pRef)
        {
            this.mainSymbol = mainSymbol;
            this.priceType = priceType;
            this.metadata = metadata;

            Init();
        }

        public string MainSymbol => mainSymbol;
        public BarPriceType PriceType => priceType;

        protected override InputSetup CreateInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInput(descriptor, mainSymbol, priceType, metadata);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInput(descriptor, mainSymbol, priceType, metadata);
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
                return new ColoredLineOutputSetup(descriptor, Algo.Common.Model.Setup.MsgCodes.UnsupportedPropertyType);
        }
    }
}
