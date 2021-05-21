using System;
using System.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1.Metadata;

namespace TickTrader.Algo.CoreV1
{
    internal class SimplifiedBuilder : PluginBuilder
    {
        private bool _isInitialized;

        public SimplifiedBuilder(PluginMetadata descriptor) : base(descriptor)
        {
            InitContext();
        }

        protected override Exception InvokeMethod<T>(Action<PluginBuilder, T> invokeAction, T param)
        {
            Exception pluginException = null;

            OnBeforeInvoke();
            
            try
            {
                invokeAction(this, param);
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                pluginException = ex;
            }
            
            OnAfterInvoke();

            return pluginException;
        }

        public void InitContext()
        {
            SynchronizationContext.SetSynchronizationContext(syncContext);
            AlgoPlugin.staticContext = this;
            _isInitialized = true;
        }

        public void DeinitContext()
        {
            if (_isInitialized)
            {
                AlgoPlugin.staticContext = null;
                SynchronizationContext.SetSynchronizationContext(null);
                _isInitialized = false;
            }
        }
    }
}
