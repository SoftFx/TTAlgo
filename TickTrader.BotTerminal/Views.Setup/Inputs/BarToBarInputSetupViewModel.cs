using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class BarToBarInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;


        protected override MappingKey DefaultMapping => _defaultMapping;


        public BarToBarInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = setupMetadata.Context.DefaultMapping;
            AvailableMappings = setupMetadata.Mappings.BarToBarMappings;
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


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToBarMappingOrDefault(mappingKey);
        }
    }
}
