using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class IndicatorSetupViewModel : PluginSetupViewModel
    {
        public override bool AllowChangeTimeFrame => false;

        public override bool AllowChangeMainSymbol => false;

        public override bool AllowChangeMapping => false;


        public IndicatorSetupViewModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
            : this(pRef, metadata, context, PluginSetupMode.New)
        {
        }

        public IndicatorSetupViewModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context, PluginSetupMode mode)
            : base(pRef, metadata, context, mode)
        {
            Init();
        }


        public override void Reset()
        {
            base.Reset();

            Permissions = new PluginPermissions
            {
                TradeAllowed = false,
                Isolated = false,
            };
        }


        protected override PluginConfig SaveToConfig()
        {
            return new IndicatorConfig();
        }
    }
}
