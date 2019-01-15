using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteInputSetupModel : InputSetupModel
    {
        private bool _useL2;

        public override string ValueAsText => SelectedSymbol.Name + ".Quote" + (_useL2 ? "L2" : "");

        public QuoteInputSetupModel(InputMetadata metadata, ISymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext, bool useL2)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
            _useL2 = useL2;
        }

        public override void Apply(IPluginSetupTarget target)
        {
            //if (useL2)
            //    target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
            //else
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Quote>(Metadata.Id, SelectedSymbol.Id, b => b);
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
