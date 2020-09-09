using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class QuoteInputSetupModel : InputSetupModel
    {
        private bool _useL2;

        public override string ValueAsText => SelectedSymbol.Name + ".Quote" + (_useL2 ? "L2" : "");

        public QuoteInputSetupModel(InputMetadata metadata, ISetupSymbolInfo mainSymbol, IAlgoSetupMetadata setupMetadata, IAlgoSetupContext setupContext, bool useL2)
            : base(metadata, mainSymbol, setupMetadata, setupContext)
        {
            _useL2 = useL2;
        }

        public override void Apply(IPluginSetupTarget target)
        {
            //if (useL2)
            //    target.MapInput<QuoteEntity, Api.Quote>(Descriptor.Id, SymbolCode, b => b);
            //else
            target.GetFeedStrategy<QuoteStrategy>().MapInput<Api.Quote>(Metadata.Id, SelectedSymbol.Id, q => new QuoteEntity(q));
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
