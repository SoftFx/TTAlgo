using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToDoubleInputSetupModel : MappedInputSetupModel
    {
        public QuoteToDoubleInputSetupModel(InputDescriptor descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode, string defaultMapping)
            : base(descriptor, metadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = metadata.SymbolMappings.QuoteToDoubleMappings;
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


        protected override SymbolMapping GetMapping(string mappingKey)
        {
            return Metadata.SymbolMappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
