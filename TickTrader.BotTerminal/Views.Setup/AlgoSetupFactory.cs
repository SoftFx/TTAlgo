using System;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public static class AlgoSetupFactory
    {
        public static PluginSetupViewModel CreateSetup(AlgoPluginRef catalogItem, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
        {
            switch (catalogItem.Descriptor.AlgoLogicType)
            {
                case AlgoTypes.Robot: return new TradeBotSetupViewModel(catalogItem, metadata, context);
                case AlgoTypes.Indicator: return new IndicatorSetupViewModel(catalogItem, metadata, context);
                default: throw new ArgumentException("Unknown plugin type");
            }
        }
    }
}
