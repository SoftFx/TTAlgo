using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public static class PluginStateHelper
    {
        public static bool IsStarted(PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Starting || state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting || state == PluginModelInfo.Types.PluginState.Stopping;
        }

        public static bool IsRunning(PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting;
        }

        public static bool IsStopped(PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Stopped || state == PluginModelInfo.Types.PluginState.Broken || state == PluginModelInfo.Types.PluginState.Faulted;
        }

        public static bool CanStop(PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Running || state == PluginModelInfo.Types.PluginState.Reconnecting;
        }

        public static bool CanStart(PluginModelInfo.Types.PluginState state)
        {
            return state == PluginModelInfo.Types.PluginState.Stopped || state == PluginModelInfo.Types.PluginState.Faulted;
        }
    }
}
