using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public abstract class MappedInputSetupViewModel : InputSetupViewModel
    {
        private MappingInfo _selectedMapping;


        protected abstract MappingKey DefaultMapping { get; }


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


        public MappedInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata)
            : base(descriptor, setupMetadata)
        {
        }


        public override void Reset()
        {
            base.Reset();

            SelectedMapping = GetMapping(DefaultMapping);
        }


        protected abstract MappingInfo GetMapping(MappingKey mappingKey);


        protected override void LoadConfig(Input input)
        {
            var mappedInput = input as MappedInput;
            SelectedMapping = GetMapping(mappedInput?.SelectedMapping ?? DefaultMapping);

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
