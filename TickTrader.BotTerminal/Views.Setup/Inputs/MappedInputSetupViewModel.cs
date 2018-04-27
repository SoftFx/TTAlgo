using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public abstract class MappedInputSetupViewModel : InputSetupViewModel
    {
        private MappingKey _defaultMapping;
        private MappingInfo _selectedMapping;


        public IReadOnlyList<MappingInfo> AvailableMappings { get; protected set; }

        public MappingInfo SelectedMapping
        {
            get { return _selectedMapping; }
            set
            {
                if (_selectedMapping == value)
                    return;

                _selectedMapping = value;
                NotifyOfPropertyChange(nameof(SelectedMapping));
            }
        }


        public MappedInputSetupViewModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode, MappingKey defaultMapping)
            : base(metadata, accountMetadata, defaultSymbolCode)
        {
            _defaultMapping = defaultMapping;
        }


        public override void Reset()
        {
            base.Reset();

            SelectedMapping = GetMapping(_defaultMapping);
        }


        protected abstract MappingInfo GetMapping(MappingKey mappingKey);


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
                mappedInput.SelectedMapping = SelectedMapping.Key;
            }

            base.SaveConfig(input);
        }
    }
}
