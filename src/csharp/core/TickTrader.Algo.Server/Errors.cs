using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal static class Errors
    {
        public static Exception DuplicatePlugin(string pluginId) => new AlgoException($"Plugin '{pluginId}' already exists");

        public static Exception PluginNotFound(string pluginId) => new AlgoException($"Plugin '{pluginId}' not found");

        public static Exception DuplicateExecutorId(string executorId) => new AlgoException($"Executor '{executorId}' already exists");
    }
}
