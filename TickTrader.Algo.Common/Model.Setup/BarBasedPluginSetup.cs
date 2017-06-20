using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarBasedPluginSetup : PluginSetup
    {
        private string mainSymbol;
        private BarPriceType priceType;
        private IAlgoGuiMetadata metadata;

        public BarBasedPluginSetup(AlgoPluginRef pRef)
            : base(pRef)
        {
            Init();
        }

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

        public override void Load(PluginConfig cfg)
        {
            var barConfig = cfg as BarBasedConfig;
            if (barConfig != null)
            {
                mainSymbol = barConfig.MainSymbol;
                priceType = barConfig.PriceType;       
            }

            base.Load(cfg);
        }

        protected override PluginConfig SaveToConfig()
        {
            var config = new BarBasedConfig();
            config.MainSymbol = MainSymbol;
            config.PriceType = PriceType;
            return config;
        }
    }
}
