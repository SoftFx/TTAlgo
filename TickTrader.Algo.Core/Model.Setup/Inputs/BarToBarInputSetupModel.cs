using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToBarInputSetupModel : MappedInputSetupModel
    {
        public override string EntityPrefix => "Bar";

        public BarToBarInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as BarToBarInputConfig;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new BarToBarInputConfig();
            SaveConfig(input);
            return input;
        }


        protected override Mapping GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToBarMappingOrDefault(mappingKey);
        }
    }
}
