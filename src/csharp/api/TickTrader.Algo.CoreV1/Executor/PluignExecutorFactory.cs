using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class PluignExecutorFactory
    {
        private readonly PluginKey _pluginKey;


        public PluignExecutorFactory(PluginKey pluginKey)
        {
            _pluginKey = pluginKey;
        }


        public PluginExecutorCore CreateExecutor(string instanceId)
        {
            return new PluginExecutorCore(_pluginKey, instanceId);
        }
    }
}
