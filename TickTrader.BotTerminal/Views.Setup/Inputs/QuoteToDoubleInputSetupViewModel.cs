using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.BotTerminal
{
    public class QuoteToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;


        protected override MappingKey DefaultMapping => _defaultMapping;


        public QuoteToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = setupMetadata.Mappings.DefaultQuoteToDoubleMapping.Key;
            AvailableMappings = setupMetadata.Mappings.QuoteToDoubleMappings;
        }


        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as QuoteToDoubleInputConfig;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new QuoteToDoubleInputConfig();
            SaveConfig(input);
            return input;
        }

        public override void Reset()
        {
            base.Reset();

            SelectedSymbol = SetupMetadata.DefaultSymbol.ToKey(); // Quote inputs does not support Main Symbol token
        }

        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
