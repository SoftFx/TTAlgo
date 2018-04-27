using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class BarToBarInputSetupModel : MappedInputSetupModel
    {
        public BarToBarInputSetupModel(InputMetadata descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode, string defaultMapping)
            : base(descriptor, metadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = metadata.SymbolMappings.BarToBarMappings;
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


        protected override SymbolMapping GetMapping(string mappingKey)
        {
            return Metadata.SymbolMappings.GetBarToBarMappingOrDefault(mappingKey);
        }
    }
}
