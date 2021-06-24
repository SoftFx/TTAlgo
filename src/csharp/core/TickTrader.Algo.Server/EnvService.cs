using System.IO;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    public class EnvService
    {
        public string AppFolder { get; }

        public string FeedHistoryCacheFolder { get; }

        public string BotLogFolder { get; }

        public string LogFolder { get; }

        public string AlgoRepositoryFolder { get; }

        public string AlgoExtFolder { get; }

        public string AlgoWorkingFolder { get; }

        public string AppDataFolder { get; }

        public string ServerStateFilePath { get; }


        public EnvService(string appFolder)
        {
            AppFolder = appFolder;

            BotLogFolder = Path.Combine(appFolder, "BotLogs");
            LogFolder = Path.Combine(appFolder, "Logs");
            AlgoRepositoryFolder = Path.Combine(appFolder, "AlgoRepository");
            AlgoExtFolder = Path.Combine(appFolder, "AlgoExt");
            AlgoWorkingFolder = Path.Combine(appFolder, "AlgoData");
            FeedHistoryCacheFolder = Path.Combine(appFolder, "FeedCache");
            AppDataFolder = Path.Combine(appFolder, "Settings");
            ServerStateFilePath = Path.Combine(AppDataFolder, "server.state.json");

            PathHelper.EnsureDirectoryCreated(appFolder);
            PathHelper.EnsureDirectoryCreated(AlgoRepositoryFolder);
            PathHelper.EnsureDirectoryCreated(AlgoExtFolder);
            PathHelper.EnsureDirectoryCreated(AlgoWorkingFolder);
            PathHelper.EnsureDirectoryCreated(BotLogFolder);
            PathHelper.EnsureDirectoryCreated(LogFolder);
            PathHelper.EnsureDirectoryCreated(FeedHistoryCacheFolder);
            PathHelper.EnsureDirectoryCreated(AppDataFolder);
        }


        public string GetPluginWorkingFolder(string pluginId)
        {
            return Path.Combine(AlgoWorkingFolder, PathEscaper.Escape(pluginId));
        }

        public string GetPluginLogsFolder(string pluginId)
        {
            return Path.Combine(BotLogFolder, PathEscaper.Escape(pluginId));
        }
    }
}
