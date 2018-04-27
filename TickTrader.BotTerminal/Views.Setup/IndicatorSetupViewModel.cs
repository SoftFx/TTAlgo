using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class IndicatorSetupViewModel : PluginSetupViewModel
    {
        public override bool AllowChangeTimeFrame => false;

        public override bool AllowChangeMainSymbol => false;

        public override bool AllowChangeMapping => false;


        public IndicatorSetupViewModel(PluginInfo plugin, SetupMetadataInfo metadata, SetupContextInfo context, AccountMetadataInfo account)
            : this(plugin, metadata, context, account, PluginSetupMode.New)
        {
        }

        public IndicatorSetupViewModel(PluginInfo plugin, SetupMetadataInfo metadata, SetupContextInfo context, AccountMetadataInfo account, PluginSetupMode mode)
            : base(plugin, metadata, context, account, mode)
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
