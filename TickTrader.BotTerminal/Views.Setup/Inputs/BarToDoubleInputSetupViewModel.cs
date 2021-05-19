using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class BarToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;

        protected override MappingKey DefaultMapping => _defaultMapping;

        public BarToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = setupMetadata.Mappings.DefaultBarToDoubleMapping.Key;
            AvailableMappings = setupMetadata.Mappings.BarToDoubleMappings;
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
