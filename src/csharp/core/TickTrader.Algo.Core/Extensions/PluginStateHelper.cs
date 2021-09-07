using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class PluginStateHelper
    {
        public static bool IsStarted(this PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Starting || state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting || state == PluginModelInfo.Types.PluginState.Stopping;
        }

        public static bool IsRunning(this PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting;
        }

        public static bool IsStopped(this PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Stopped || state == PluginModelInfo.Types.PluginState.Broken || state == PluginModelInfo.Types.PluginState.Faulted;
        }

        public static bool CanStop(this PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting;
        }

        public static bool CanStart(this PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Stopped || state == PluginModelInfo.Types.PluginState.Faulted;
        }
    }
}
