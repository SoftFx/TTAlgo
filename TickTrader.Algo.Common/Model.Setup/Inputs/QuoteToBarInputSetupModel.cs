using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarInputSetupModel : MappedInputSetupModel
    {
        public QuoteToBarInputSetupModel(InputDescriptor descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode, string defaultMapping)
            : base(descriptor, metadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = metadata.SymbolMappings.QuoteToBarMappings;
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


        protected override SymbolMapping GetMapping(string mappingKey)
        {
            return Metadata.SymbolMappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
