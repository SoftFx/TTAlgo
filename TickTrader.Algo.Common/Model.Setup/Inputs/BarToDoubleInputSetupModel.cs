using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToDoubleInputSetupModel : MappedInputSetupModel
    {
        public BarToDoubleInputSetupModel(InputMetadata metadata, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, setupMetadata, setupContext)
        {
        }


        public override void Load(Property srcProperty)
        {
            var input = srcProperty as BarToBarInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new BarToBarInput();
            SaveConfig(input);
            return input;
        }


        protected override Mapping GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToDoubleMappingOrDefault(mappingKey);
        }
    }
}
