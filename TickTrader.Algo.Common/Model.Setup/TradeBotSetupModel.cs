using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class TradeBotSetupModel : PluginSetupModel
    {
        public override bool AllowChangeTimeFrame => true;

        public override bool AllowChangeMainSymbol => true;

        public override bool AllowChangeMapping => true;


        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
            : this(pRef, metadata, context, PluginSetupMode.New)
        {
        }

        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context, PluginSetupMode mode)
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

        public override object Clone(PluginSetupMode mode)
        {
            var config = Save();
            var setupModel = new TradeBotSetupModel(PluginRef, Metadata, Context, mode);
            setupModel.Load(config);
            return setupModel;
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
