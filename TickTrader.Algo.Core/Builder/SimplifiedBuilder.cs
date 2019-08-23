using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
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
