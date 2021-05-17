using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class MappedInputSetupModel : InputSetupModel
    {
        public MappingInfo SelectedMapping { get; protected set; }

        public abstract string EntityPrefix { get; }

        public override string ValueAsText => SelectedSymbol.Name + "." + EntityPrefix + "." + SelectedMapping.DisplayName;

        public MappedInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
        }

        public override void Apply(IPluginSetupTarget target)
        {
            //target.MapInput(Metadata.Id, SelectedSymbol.Id, SelectedMapping);
        }

        public override void Reset()
        {
            base.Reset();

            SelectedMapping = GetMapping(SetupContext.DefaultMapping);
        }

        protected abstract MappingInfo GetMapping(MappingKey mappingKey);

        protected override void LoadConfig(IInputConfig input)
        {
            var mappedInput = input as IMappedInputConfig;
            SelectedMapping = GetMapping(mappedInput?.SelectedMapping ?? SetupContext.DefaultMapping);

            base.LoadConfig(input);
        }

        protected override void SaveConfig(IInputConfig input)
        {
            var mappedInput = input as IMappedInputConfig;
            if (mappedInput != null)
            {
                mappedInput.SelectedMapping = SelectedMapping.Key;
            }

            base.SaveConfig(input);
        }
    }
}
