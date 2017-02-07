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
using TickTrader.Algo.GuiModel;
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
        PluginSetup CreateSetup(AlgoPluginRef catalogItem);
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
        string SymbolCode { get; }
        TimeFrames TimeFrame { get; }
        DateTime TimelineStart { get; }
        BotJournal Journal { get; }

        //BotJournal Journal { get; }
        //FeedModel Feed { get; }
        //TraderModel Trade { get; }

        IAccountInfoProvider GetAccInfoProvider();
        void InitializePlugin(PluginExecutor plugin);
        ITradeApi GetTradeApi();

        event Action ParamsChanged;
        event Action StartEvent;
        event AsyncEventHandler StopEvent;

        //event Action<PluginCatalogItem> PluginBeingReplaced; // fired on background thread!
        //event Action<PluginCatalogItem> PluginBeingRemoved; // fired on background thread!
    }
}
