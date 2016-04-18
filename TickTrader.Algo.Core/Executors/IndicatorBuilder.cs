using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    /// <summary>
    /// Note: This class is not thread safe and designed to be used from one single thread
    /// </summary>
    public class IndicatorBuilder : PluginExecutor
    {
        private IndicatorAdapter pluginProxy;

        public IndicatorBuilder(AlgoPluginDescriptor descriptor)
            : base(descriptor)
        {
            pluginProxy = PluginAdapter.CreateIndicator(descriptor.Id, this);
        }

        internal override PluginAdapter PluginProxy { get { return pluginProxy; } }

        public void BuildNext(int count = 1)
        {
            BuildNext(count, CancellationToken.None);
        }

        public void BuildNext(int count, CancellationToken cToken)
        {
            LazyInit();
            pluginProxy.Coordinator.FireBeginBatch();
            for (int i = 0; i < count; i++)
            {
                if (cToken.IsCancellationRequested)
                    return;
                pluginProxy.Coordinator.MoveNext();
                pluginProxy.Calculate(false);
            }
            pluginProxy.Coordinator.FireEndBatch();
        }

        public void RebuildLast()
        {
            LazyInit();
            pluginProxy.Calculate(true);
        }
    }
}
