using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    public static class PluginStateHelper
    {
        public static bool IsStarted(PluginStates state)
        {
            return state == PluginStates.Starting || state == PluginStates.Running || state == PluginStates.Reconnecting || state == PluginStates.Stopping;
        }

        public static bool IsRunning(PluginStates state)
        {
            return state == PluginStates.Running || state == PluginStates.Reconnecting;
        }

        public static bool IsStopped(PluginStates state)
        {
            return state == PluginStates.Stopped || state == PluginStates.Broken || state == PluginStates.Faulted;
        }

        public static bool CanStop(PluginStates state)
        {
            return state == PluginStates.Running || state == PluginStates.Reconnecting;
        }

        public static bool CanStart(PluginStates state)
        {
            return state == PluginStates.Stopped || state == PluginStates.Faulted;
        }
    }
}
