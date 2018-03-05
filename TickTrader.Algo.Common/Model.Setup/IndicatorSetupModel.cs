using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class IndicatorSetupModel : PluginSetupModel
    {
        public override bool AllowChangeTimeFrame => false;

        public override bool AllowChangeMainSymbol => false;

        public override bool AllowChangeMapping => false;


        public IndicatorSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping) : this(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping, PluginSetupMode.New)
        {
        }

        public IndicatorSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, string defaultSymbolCode, TimeFrames defaultTimeFrame,
            string defaultMapping, PluginSetupMode mode) : base(pRef, metadata, defaultSymbolCode, defaultTimeFrame, defaultMapping, mode)
        {
            Init();
        }


        public override object Clone(PluginSetupMode mode)
        {
            var config = Save();
            var setupModel = new IndicatorSetupModel(PluginRef, Metadata, DefaultSymbolCode, DefaultTimeFrame, DefaultMapping, mode);
            setupModel.Load(config);
            return setupModel;
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
