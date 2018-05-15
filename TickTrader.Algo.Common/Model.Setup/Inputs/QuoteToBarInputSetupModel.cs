using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteToBarInputSetupModel : MappedInputSetupModel
    {
        public QuoteToBarInputSetupModel(InputMetadata metadata, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, setupMetadata, setupContext)
        {
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


        protected override Mapping GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
