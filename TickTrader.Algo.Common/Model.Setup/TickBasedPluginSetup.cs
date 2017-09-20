using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class TickBasedPluginSetup : PluginSetup
    {
        private string _mainSymbol;


        public string MainSymbol => _mainSymbol;


        public TickBasedPluginSetup(AlgoPluginRef pRef, string mainSymbol)
            : base(pRef)
        {
            _mainSymbol = mainSymbol;

            Init();
        }


        protected override InputSetup CreateInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetup.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetup(descriptor, _mainSymbol);
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetup(descriptor, _mainSymbol, Api.BarPriceType.Bid);
                case "TickTrader.Algo.Api.Quote": return new QuoteToQuoteInputSetup(descriptor, _mainSymbol, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteToQuoteInputSetup(descriptor, _mainSymbol, true);
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
                return new ColoredLineOutputSetup(descriptor, MsgCodes.UnsupportedPropertyType);
        }

        public override void Load(PluginConfig cfg)
        {
            var quoteConfig = cfg as QuoteBasedConfig;
            if (quoteConfig != null)
            {
                _mainSymbol = quoteConfig.MainSymbol;
            }

            base.Load(cfg);
        }

        protected override PluginConfig SaveToConfig()
        {
            var config = new QuoteBasedConfig()
            {
                MainSymbol = MainSymbol,
            };
            return config;
        }

        public override object Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
