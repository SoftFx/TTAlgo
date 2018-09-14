using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class MappedInputSetupModel : InputSetupModel
    {
        public Mapping SelectedMapping { get; protected set; }


        public MappedInputSetupModel(InputMetadata metadata, ISymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
        }


        public override void Apply(IPluginSetupTarget target)
        {
            SelectedMapping?.MapInput(target, Metadata.Id, SelectedSymbol.Id);
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
