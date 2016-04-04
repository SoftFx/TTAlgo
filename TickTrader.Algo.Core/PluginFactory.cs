using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class PluginFactory : IPluginActivator
    {
        private Type pluginType;
        private PluginContext wrapper;

        public PluginFactory(Type algoType)
        {
            this.pluginType = algoType;
        }

        private void CreateInstance()
        {
            try
            {
                Api.AlgoPlugin.activator = this;
                Activator.CreateInstance(pluginType);
            }
            finally
            {
                Api.AlgoPlugin.activator = null;
            }
        }

        public PluginContext Create()
        {
            CreateInstance();
            return wrapper;
        }

        IPluginDataProvider IPluginActivator.Activate(AlgoPlugin instance)
        {
            if (instance is Indicator)
            {
                wrapper = new IndicatorContext(instance);
                return wrapper;
            }
            else
                throw new Exception("Unknown plugin class");
        }
    }
}
