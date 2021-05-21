using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class PluignExecutorFactory
    {
        private PluginKey _pluginKey;

        internal PluignExecutorFactory(PluginKey pluginKey)
        {
            _pluginKey = pluginKey;
        }

        public PluginExecutorCore CreateExecutor()
        {
            return new PluginExecutorCore(_pluginKey);
        }
    }
}
