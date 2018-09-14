using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class QuoteToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;


        protected override MappingKey DefaultMapping => _defaultMapping;


        public QuoteToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new MappingKey(setupMetadata.Mappings.DefaultQuoteToDoubleReduction);
            AvailableMappings = setupMetadata.Mappings.QuoteToDoubleMappings;
        }


        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToDoubleInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToDoubleInput();
            SaveConfig(input);
            return input;
        }


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
