using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class TradeBotSetupViewModel : PluginSetupViewModel
    {
        public override bool AllowChangeTimeFrame => true;

        public override bool AllowChangeMainSymbol => true;

        public override bool AllowChangeMapping => true;


        public TradeBotSetupViewModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
            : this(pRef, metadata, context, PluginSetupMode.New)
        {
        }

        public TradeBotSetupViewModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context, PluginSetupMode mode)
            : base(pRef, metadata, context, mode)
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
