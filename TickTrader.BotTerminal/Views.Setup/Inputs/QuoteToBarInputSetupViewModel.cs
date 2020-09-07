using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public class QuoteToBarInputSetupViewModel : MappedInputSetupViewModel
    {
        private Algo.Domain.MappingKey _defaultMapping;


        protected override Algo.Domain.MappingKey DefaultMapping => _defaultMapping;


        public QuoteToBarInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new Algo.Domain.MappingKey(setupMetadata.Mappings.DefaultQuoteToBarReduction);
            AvailableMappings = setupMetadata.Mappings.QuoteToBarMappings;
        }


        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToBarInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToBarInput();
            SaveConfig(input);
            return input;
        }


        protected override MappingInfo GetMapping(Algo.Domain.MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
