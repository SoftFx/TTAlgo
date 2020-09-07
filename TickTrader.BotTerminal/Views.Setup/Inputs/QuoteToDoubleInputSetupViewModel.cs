using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public class QuoteToDoubleInputSetupViewModel : MappedInputSetupViewModel
    {
        private Algo.Domain.MappingKey _defaultMapping;


        protected override Algo.Domain.MappingKey DefaultMapping => _defaultMapping;


        public QuoteToDoubleInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new Algo.Domain.MappingKey(setupMetadata.Mappings.DefaultQuoteToDoubleReduction);
            AvailableMappings = setupMetadata.Mappings.QuoteToDoubleMappings;
        }


        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteToDoubleInput;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteToDoubleInput();
            SaveConfig(input);
            return input;
        }

        public override void Reset()
        {
            base.Reset();

            SelectedSymbol = SetupMetadata.DefaultSymbol; // Quote inputs does not support Main Symbol token
        }

        protected override MappingInfo GetMapping(Algo.Domain.MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToDoubleMappingOrDefault(mappingKey);
        }
    }
}
