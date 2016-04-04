using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.DataflowConcept
{
    /// <summary>
    /// Note: PluginExecutor is always located in main AppDomain.
    /// </summary>
    public class BarIndicatorExecutor : IPluginDataProvider
    {
        private ExecutionContext marshalContext;
        private IRealtimeFeedProvider<Bar> dataPovider;

        public BarIndicatorExecutor(AlgoPluginRef pluginRef, string mainSymbolId,
            IRealtimeFeedProvider<Bar> dataPovider)
        {
            this.MarshalContext = marshalContext;
            this.dataPovider = dataPovider;
            this.marshalContext = pluginRef.CreateContext();
        }

        public ExecutionContext MarshalContext { get; private set; }

        //private class Builder : MarshalByRefObject
        //{
        //    private PluginContext plugin;

        //    public void InitIdicator(string pluginId)
        //    {
        //        plugin = PluginContext.Create(pluginId, this);
        //    }
        //}

        OrderList IPluginDataProvider.GetOrdersCollection()
        {
            throw new NotImplementedException();
        }

        PositionList IPluginDataProvider.GetPositionsCollection()
        {
            throw new NotImplementedException();
        }

        MarketSeries IPluginDataProvider.GetMainMarketSeries()
        {
            throw new NotImplementedException();
        }

        Leve2Series IPluginDataProvider.GetMainLevel2Series()
        {
            throw new NotImplementedException();
        }
    }

    internal interface IPluginExecAgent
    {
    }
}
