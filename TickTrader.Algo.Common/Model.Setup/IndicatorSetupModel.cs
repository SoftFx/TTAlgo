using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public class IndicatorSetupModel : PluginSetupModel
    {
        public IndicatorSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
            : base(pRef, metadata, context)
        {
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
