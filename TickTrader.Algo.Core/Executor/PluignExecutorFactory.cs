using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class PluignExecutorFactory : CrossDomainObject
    {
        private string _pluginId;

        internal PluignExecutorFactory(string pluginId)
        {
            _pluginId = pluginId;
        }

        public PluginExecutorCore CreateExecutor()
        {
            return new PluginExecutorCore(_pluginId);
        }
    }
}
