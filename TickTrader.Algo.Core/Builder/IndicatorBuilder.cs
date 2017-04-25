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
    /// Note: This is simplified method to build indicators. It does not support multithreading indicators.
    /// </summary>
    public class IndicatorBuilder : PluginBuilder
    {
        private bool isInitialized;

        public IndicatorBuilder(AlgoPluginDescriptor descriptor)
            : base(descriptor)
        {
        }

        public void BuildNext(int count = 1)
        {
            BuildNext(count, CancellationToken.None);
        }

        public void BuildNext(int count, CancellationToken cToken)
        {
            LazyInit();
            PluginProxy.Coordinator.BeginBatch();
            for (int i = 0; i < count; i++)
            {
                if (cToken.IsCancellationRequested)
                    return;
                PluginProxy.Coordinator.Extend();
                InvokeCalculate(false);
            }
            PluginProxy.Coordinator.EndBatch();
        }

        public void RebuildLast()
        {
            LazyInit();
            InvokeCalculate(true);
        }

        private void LazyInit()
        {
            if (isInitialized)
                return;

            PluginProxy.InvokeInit();

            isInitialized = true;
        }
    }
}
