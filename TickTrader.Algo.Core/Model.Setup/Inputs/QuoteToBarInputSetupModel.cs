using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarInputSetupModel : MappedInputSetupModel
    {
        public override string EntityPrefix => "Quote";

        public QuoteToBarInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
            SelectedSymbol = mainSymbol; // Quotes Input does not support Main Symbol token
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as QuoteToBarInputConfig;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new QuoteToBarInputConfig();
            SaveConfig(input);
            return input;
        }


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
