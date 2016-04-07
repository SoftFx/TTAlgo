using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal interface IIndicatorSetup
    {
        long InstanceId { get; }
        int DataLen { get; }
        AlgoPluginDescriptor Descriptor { get; }
        IndicatorSetupBase UiModel { get; }
        IndicatorModel CreateIndicator();
        IIndicatorSetup CreateCopy();
    }

    internal interface IIndicatorHost
    {
        IIndicatorSetup CreateIndicatorConfig(AlgoCatalogItem catalogItem);
        void AddOrUpdateIndicator(IIndicatorSetup cfg);
    }
}
