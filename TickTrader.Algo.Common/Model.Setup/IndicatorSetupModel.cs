using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class IndicatorSetupModel : PluginSetupModel
    {
        public override bool AllowChangeTimeFrame => false;

        public override bool AllowChangeMainSymbol => false;

        public override bool AllowChangeMapping => false;


        public IndicatorSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping) : base(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping)
        {
            Init();
        }


        public override void Load(PluginConfig cfg)
        {
            var indicatorConfig = cfg as IndicatorConfig;
            if (indicatorConfig != null)
            {

            }

            base.Load(cfg);
        }

        public override object Clone()
        {
            var config = Save();
            var setupModel = new IndicatorSetupModel(PluginRef, Metadata, DefaultSymbolCode, DefaultTimeFrame, DefaultMapping);
            setupModel.Load(config);
            return setupModel;
        }


        protected override PluginConfig SaveToConfig()
        {
            return new IndicatorConfig();
        }
    }
}
