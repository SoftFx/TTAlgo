using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public static class Errors
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

        public static Exception PluginFileNotFound(string fileName) => new AlgoException($"File with name '{fileName}' not found");

        public static Exception PluginLogsFolderUploadForbidden() => new AlgoException("Uploading files to plugin logs folder is not allowed");

        public static Exception PluginFileIncorrectName(string fileName) => new AlgoException($"Incorrect file name '{fileName}'");

        public static Exception DuplicateLocationId(string locationId) => new AlgoException($"Cannot register multiple paths for location '{locationId}'");

        public static Exception MissingPathForUploadLocation() => new AlgoException($"Path for upload locationId not found");

        public static Exception MissingSynchronizationContext() => new AlgoException("SynchronizationContext is not found");

        public static Exception SynchronizationContextIsDifferent() => new AlgoException("Can't change object outside of original SynchronizationContext");

        public static Exception PackageNotFound(string pkgId) => new AlgoException($"Package '{pkgId}' not found");

        public static Exception PkgRefNotFound(string pkgRefId) => new AlgoException($"Package ref '{pkgRefId}' not found");

        public static Exception PackageLocked(string pkgId) => new AlgoException($"One or more trade bots from package '{pkgId}' is being executed! Please stop all bots and try again!");
    }
}
