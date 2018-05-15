using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class TradeBotSetupModel : PluginSetupModel
    {
        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
            : base(pRef, metadata, context)
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
