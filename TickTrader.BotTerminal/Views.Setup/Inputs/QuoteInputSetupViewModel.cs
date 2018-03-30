using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    public class QuoteInputSetupViewModel : InputSetupViewModel
    {
        private bool _useL2;


        public QuoteInputSetupViewModel(InputMetadataInfo metadata, AccountMetadataInfo accountMetadata, string defaultSymbolCode, bool useL2)
            : base(metadata, accountMetadata, defaultSymbolCode)
        {
            _useL2 = useL2;
        }


        public override void Load(Property srcProperty)
        {
            var input = srcProperty as QuoteInput;
            if (input != null)
            {
                _useL2 = input.UseL2;
                LoadConfig(input);
            }
        }

        public override Property Save()
        {
            var input = new QuoteInput { UseL2 = _useL2 };
            SaveConfig(input);
            return input;
        }
    }
}
