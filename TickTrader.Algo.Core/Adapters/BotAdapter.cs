using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class BotAdapter : PluginAdapter
    {
        internal BotAdapter(Func<AlgoPlugin> pluginFactory, IPluginDataProvider provider)
            : base(pluginFactory, provider, new BuffersCoordinator())
        {
            InitParameters();
            BindUpInputs();
            BindUpOutputs();
        }

        public void InvokeStart()
        {
            ((TradeBot)PluginInstance).InvokeStart();
        }

        public void InvokeStop()
        {
            ((TradeBot)PluginInstance).InvokeStop();
        }
    }
}
