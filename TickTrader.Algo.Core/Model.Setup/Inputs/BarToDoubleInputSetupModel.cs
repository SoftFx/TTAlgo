using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToDoubleInputSetupModel : MappedInputSetupModel
    {
        public override string EntityPrefix => "Bar";

        public BarToDoubleInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as BarToDoubleInputConfig;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new BarToDoubleInputConfig();
            SaveConfig(input);
            return input;
        }

        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToDoubleMappingOrDefault(mappingKey);
        }
    }
}
