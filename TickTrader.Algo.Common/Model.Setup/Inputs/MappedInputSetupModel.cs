using System.Collections.Generic;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class MappedInputSetupModel : InputSetupModel
    {
        private string _defaultMapping;
        private Mapping _selectedMapping;


        public IReadOnlyList<Mapping> AvailableMappings { get; protected set; }

        public Mapping SelectedMapping
        {
            get { return _selectedMapping; }
            set
            {
                if (_selectedMapping == value)
                    return;

                _selectedMapping = value;
                NotifyPropertyChanged(nameof(SelectedMapping));
            }
        }


        public MappedInputSetupModel(InputMetadata descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode, string defaultMapping)
            : base(descriptor, metadata, defaultSymbolCode)
        {
            _defaultMapping = defaultMapping;
        }


        public override void Apply(IPluginSetupTarget target)
        {
            _selectedMapping?.MapInput(target, Descriptor.Id, SelectedSymbol.Name);
        }

        public override void Reset()
        {
            base.Reset();

            SelectedMapping = GetMapping(_defaultMapping);
        }


        protected abstract Mapping GetMapping(string mappingKey);


        protected override void LoadConfig(Input input)
        {
            var mappedInput = input as MappedInput;
            SelectedMapping = GetMapping(mappedInput?.SelectedMapping ?? _defaultMapping);

            base.LoadConfig(input);
        }

        protected override void SaveConfig(Input input)
        {
            var mappedInput = input as MappedInput;
            if (mappedInput != null)
            {
                mappedInput.SelectedMapping = SelectedMapping.DisplayName;
            }

            base.SaveConfig(input);
        }
    }
}
