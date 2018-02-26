using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class TradeBotSetupModel : PluginSetupModel
    {
        public override bool AllowChangeTimeFrame => true;

        public override bool AllowChangeMainSymbol => true;

        public override bool AllowChangeMapping => true;


        public TradeBotSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping) : base(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping)
        {
            Init();
        }


        public override void Load(PluginConfig cfg)
        {
            var botConfig = cfg as TradeBotConfig;
            if (botConfig != null)
            {

            }

            base.Load(cfg);
        }

        public override object Clone()
        {
            var config = Save();
            var setupModel = new TradeBotSetupModel(PluginRef, Metadata, DefaultSymbolCode, DefaultTimeFrame, DefaultMapping);
            setupModel.Load(config);
            return setupModel;
        }


        protected override PluginConfig SaveToConfig()
        {
            return new TradeBotConfig();
        }
    }
}
