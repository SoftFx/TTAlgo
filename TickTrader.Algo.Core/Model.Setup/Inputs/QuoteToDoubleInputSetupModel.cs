using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToDoubleInputSetupModel : MappedInputSetupModel
    {
        public override string EntityPrefix => "Quote";

        public QuoteToDoubleInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
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

        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
