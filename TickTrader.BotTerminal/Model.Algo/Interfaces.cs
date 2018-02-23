using Machinarium.Qnil;
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
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    //internal interface IIndicatorSetup
    //{
    //    long InstanceId { get; }
    //    int DataLen { get; }
    //    AlgoPluginDescriptor Descriptor { get; }
    //    PluginSetup UiModel { get; }
    //    IndicatorModel CreateIndicator();
    //    IIndicatorSetup CreateCopy();
    //}

    //internal interface IAlgoFactory
    //{
    //}

    internal interface IAlgoSetupFactory
    {
        PluginSetupModel CreateSetup(AlgoPluginRef catalogItem);
    }

    //internal interface IIndicatorAdapterContext
    //{
    //    OutputBuffer<T> GetOutput<T>(string name);
    //    DateTime GetTimeCoordinate(int index);
    //    void AddSeries(IRenderableSeries series);
    //    void AddSeries(DynamicList<MarkerAnnotation> series);
    //}

    internal interface IAlgoPluginHost
    {
        void Lock();
        void Unlock();

        bool IsStarted { get; }
        void InitializePlugin(PluginExecutor plugin);
        void UpdatePlugin(PluginExecutor plugin);

        ITradeExecutor GetTradeApi();
        ITradeHistoryProvider GetTradeHistoryApi();
        BotJournal Journal { get; }

        event Action ParamsChanged;
        event Action Connected;
        event Action StartEvent;
        event AsyncEventHandler StopEvent;

        //event Action<PluginCatalogItem> PluginBeingReplaced; // fired on background thread!
        //event Action<PluginCatalogItem> PluginBeingRemoved; // fired on background thread!
    }
}
