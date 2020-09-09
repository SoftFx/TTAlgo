using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class QuoteInputSetupViewModel : InputSetupViewModel
    {
        private bool _useL2;


        public QuoteInputSetupViewModel(InputDescriptor descriptor, SetupMetadata setupMetadata, bool useL2)
            : base(descriptor, setupMetadata)
        {
            _useL2 = useL2;
        }


        public override void Load(IPropertyConfig srcProperty)
        {
            var input = srcProperty as QuoteInputConfig;
            if (input != null)
            {
                _useL2 = input.UseL2;
                LoadConfig(input);
            }
        }

        public override IPropertyConfig Save()
        {
            var input = new QuoteInputConfig { UseL2 = _useL2 };
            SaveConfig(input);
            return input;
        }
    }
}
