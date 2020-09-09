﻿using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class QuoteToBarInputSetupViewModel : MappedInputSetupViewModel
    {
        private MappingKey _defaultMapping;


        protected override MappingKey DefaultMapping => _defaultMapping;


        public QuoteToBarInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
            _defaultMapping = new MappingKey(setupMetadata.Mappings.DefaultQuoteToBarReduction);
            AvailableMappings = setupMetadata.Mappings.QuoteToBarMappings;
        }


        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as QuoteToBarInputConfig;
            if (input != null)
            {
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new QuoteToBarInputConfig();
            SaveConfig(input);
            return input;
        }


        protected override MappingInfo GetMapping(MappingKey mappingKey)
        {
            return SetupMetadata.Mappings.GetQuoteToBarMappingOrDefault(mappingKey);
        }
    }
}
