﻿using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class MappedInputSetupModel : InputSetupModel
    {
        public Mapping SelectedMapping { get; protected set; }

        public abstract string EntityPrefix { get; }

        public override string ValueAsText => SelectedSymbol.Name + "." + EntityPrefix + "." + SelectedMapping.DisplayName;

        public MappedInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput(Metadata.Id, SelectedSymbol.Id, SelectedMapping);
        }

        public override void Reset()
        {
            base.Reset();

            SelectedMapping = GetMapping(SetupContext.DefaultMapping);
        }

        protected abstract Mapping GetMapping(MappingKey mappingKey);

        protected override void LoadConfig(Input input)
        {
            var mappedInput = input as MappedInput;
            SelectedMapping = GetMapping(mappedInput?.SelectedMapping ?? SetupContext.DefaultMapping);

            base.LoadConfig(input);
        }

        protected override void SaveConfig(Input input)
        {
            var mappedInput = input as MappedInput;
            if (mappedInput != null)
            {
                mappedInput.SelectedMapping = SelectedMapping.Key;
            }

            base.SaveConfig(input);
        }
    }
}