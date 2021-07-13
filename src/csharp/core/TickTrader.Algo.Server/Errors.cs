using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal static class Errors
    {
        public static Exception DuplicatePlugin(string pluginId) => new AlgoException($"Plugin '{pluginId}' already exists");

        public static Exception PluginNotFound(string pluginId) => new AlgoException($"Plugin '{pluginId}' not found");

        public static Exception DuplicateExecutorId(string executorId) => new AlgoException($"Executor '{executorId}' already exists");

        public static Exception ExecutorNotFound(string executorId) => new AlgoException($"Executor '{executorId}' not found");

        public static Exception DuplicateAccount(string accId) => new AlgoException($"Account '{accId}' already exists");

        public static Exception DuplicateAccountDisplayName(string displayName, string server) => new AlgoException($"Account with DisplayName '{displayName}' already exists on server '{server}'");

        public static Exception AccountNotFound(string accId) => new AlgoException($"Account '{accId}' not found");

        public static Exception InvalidPluginConfig(string pluginId) => new AlgoException($"Plugin '{pluginId}' has invalid config");

        public static Exception PluginIsRunning(string pluginId) => new AlgoException($"Plugin '{pluginId}' is running");

        public static Exception RuntimeNotStarted(string runtimeId) => new AlgoException($"Runtime '{runtimeId}' is not started");
    }
}
