using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class BarToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;


        protected override MappingKey DefaultMapping => _defaultMapping;


        public BarToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new MappingKey(setupMetadata.Context.DefaultMapping, setupMetadata.Mappings.DefaultBarToDoubleReduction);
            AvailableMappings = setupMetadata.Mappings.BarToDoubleMappings;
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


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToDoubleMappingOrDefault(mappingKey);
        }
    }
}
