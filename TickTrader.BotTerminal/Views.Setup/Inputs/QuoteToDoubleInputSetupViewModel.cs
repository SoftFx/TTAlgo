using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class QuoteToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        public QuoteToDoubleInputSetupViewModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode, MappingKey defaultMapping)
            : base(metadata, accountMetadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = accountMetadata.SymbolMappings.QuoteToDoubleMappings;
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
            return AccountMetadata.SymbolMappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
