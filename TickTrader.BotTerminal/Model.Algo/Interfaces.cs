using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
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

    internal interface IIndicatorAdapterContext
    {
        OutputBuffer<T> GetOutput<T>(string name);
        DateTime GetTimeCoordinate(int index);
        void AddSeries(IRenderableSeries series);
        void AddSeries(DynamicList<MarkerAnnotation> series);
    }
}
