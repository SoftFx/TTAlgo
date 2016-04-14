using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class BotExecutor : PluginExecutor
    {
        private BotAdapter pluginProxy;

        public BotExecutor(AlgoPluginDescriptor descriptor)
            : base(descriptor)
        {
            pluginProxy = PluginAdapter.CreateBot(descriptor.Id, this);
        }

        internal override PluginAdapter PluginProxy { get { return pluginProxy; } }
    }
}
