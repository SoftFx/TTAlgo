using System;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class AlgoSetupFactory
    {
        public static PluginSetupModel CreateSetup(AlgoPluginRef catalogItem, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
        {
            switch (catalogItem.Descriptor.AlgoLogicType)
            {
                case AlgoTypes.Robot: return new TradeBotSetupModel(catalogItem, metadata, context);
                case AlgoTypes.Indicator: return new IndicatorSetupModel(catalogItem, metadata, context);
                default: throw new ArgumentException("Unknown plugin type");
            }
        }
    }
}
