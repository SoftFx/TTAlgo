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


        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping) : this(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping, PluginSetupMode.New)
        {
        }

        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping, PluginSetupMode mode) : base(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping, mode)
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
            var setupModel = new TradeBotSetupModel(PluginRef, Metadata, DefaultSymbolCode, DefaultTimeFrame, DefaultMapping, mode);
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
