using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class TradeBotSetupViewModel : PluginSetupViewModel
    {
        public override bool AllowChangeTimeFrame => true;

        public override bool AllowChangeMainSymbol => true;

        public override bool AllowChangeMapping => true;


        public TradeBotSetupViewModel(PluginInfo plugin, SetupMetadata metadata, IPluginIdProvider idProvider)
            : this(plugin, metadata, idProvider, PluginSetupMode.New)
        {
        }

        public TradeBotSetupViewModel(PluginInfo plugin, SetupMetadata metadata, IPluginIdProvider idProvider, PluginSetupMode mode)
            : base(plugin, metadata, idProvider, mode)
        {
            Init();
        }


        public override void Load(PluginConfig cfg)
        {
            var botConfig = cfg as TradeBotConfig;
            if (botConfig != null)
            {
                InstanceId = botConfig.InstanceId;
                AllowTrade = botConfig.Permissions.TradeAllowed;
                Isolated = botConfig.Permissions.Isolated;
            }

            base.Load(cfg);
        }

        public override void Reset()
        {
            base.Reset();

            Permissions = new PluginPermissions();
        }


        protected override PluginConfig SaveToConfig()
        {
            return new TradeBotConfig();
        }
    }
}
