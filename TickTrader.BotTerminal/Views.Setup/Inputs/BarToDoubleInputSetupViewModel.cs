using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class BarToDoubleInputSetupModel : MappedInputSetupViewModel
    {
        public BarToDoubleInputSetupModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode, string defaultMapping)
            : base(metadata, accountMetadata, defaultSymbolCode, defaultMapping)
        {
            AvailableMappings = accountMetadata.SymbolMappings.BarToDoubleMappings;
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
            return AccountMetadata.SymbolMappings.GetBarToDoubleMappingOrDefault(mappingKey);
        }
    }
}
