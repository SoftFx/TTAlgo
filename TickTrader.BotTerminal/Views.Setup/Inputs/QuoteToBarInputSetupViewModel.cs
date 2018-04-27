using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class QuoteToBarInputSetupViewModel : MappedInputSetupViewModel
    {
        public QuoteToBarInputSetupViewModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode, MappingKey defaultMapping)
            : base(metadata, accountMetadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = accountMetadata.SymbolMappings.QuoteToBarMappings;
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


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return AccountMetadata.SymbolMappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
