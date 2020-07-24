using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToDoubleInputSetupModel : MappedInputSetupModel
    {
        public override string EntityPrefix => "Quote";

        public QuoteToDoubleInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
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

        protected override Mapping GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
