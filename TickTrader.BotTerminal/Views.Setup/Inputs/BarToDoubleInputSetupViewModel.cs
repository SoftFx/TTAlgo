using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public class BarToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private Algo.Domain.MappingKey _defaultMapping;

        protected override Algo.Domain.MappingKey DefaultMapping => _defaultMapping;

        public BarToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new Algo.Domain.MappingKey(setupMetadata.Context.DefaultMapping, setupMetadata.Mappings.DefaultBarToDoubleReduction);
            AvailableMappings = setupMetadata.Mappings.BarToDoubleMappings;
        }

        public override void Load(Property srcProperty)
        {
            var input = srcProperty as BarToDoubleInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new BarToDoubleInput();
            SaveConfig(input);
            return input;
        }

        protected override MappingInfo GetMapping(Algo.Domain.MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetBarToDoubleMappingOrDefault(mappingKey);
        }
    }
}
