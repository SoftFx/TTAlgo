using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteInputSetupModel : InputSetupModel
    {
        private bool _useL2;


        public QuoteInputSetupModel(InputDescriptor descriptor, IAlgoSetupMetadata metadata, string defaultSymbolCode, bool useL2)
            : base(descriptor, metadata, defaultSymbolCode)
        {
            _useL2 = useL2;
        }


        public override void Apply(IPluginSetupTarget target)
        {
            //if (useL2)
            //    target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
            //else
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Quote>(Descriptor.Id, SelectedSymbol.Name, b => b);
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
