using System;
using System.IO;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Server
{
    public class EnvService
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<EnvService>();

        private readonly Lazy<string> _botLogFolder, _logFolder, _algoRepoFolder, _algoWorkFolder,
            _appDataFolder, _serverStatePath, _updatesFolder;


        public string AppFolder { get; }

        public string BotLogFolder => _botLogFolder.Value;

        public string LogFolder => _logFolder.Value;

        public string AlgoRepositoryFolder => _algoRepoFolder.Value;

        public string AlgoWorkingFolder => _algoWorkFolder.Value;

        public string AppDataFolder => _appDataFolder.Value;

        public string ServerStateFilePath => _serverStatePath.Value;

        public string UpdatesFolder => _updatesFolder.Value;


        public EnvService(string appFolder)
        {
            AppFolder = appFolder;

            PathHelper.EnsureDirectoryCreated(appFolder);

            _botLogFolder = new Lazy<string>(() => InitSubFolder(appFolder, "BotLogs"), true);
            _logFolder = new Lazy<string>(() => InitSubFolder(appFolder, "Logs"), true);
            _algoRepoFolder = new Lazy<string>(() => InitSubFolder(appFolder, "AlgoRepository"), true);
            _algoWorkFolder = new Lazy<string>(() => InitSubFolder(appFolder, "AlgoData"), true);
            _appDataFolder = new Lazy<string>(() => InitSubFolder(appFolder, "Settings"), true);
            _serverStatePath = new Lazy<string>(() => Path.Combine(AppDataFolder, "server.state.json"), true);
            _updatesFolder = new Lazy<string>(() => InitSubFolder(appFolder, "Updates"), true);

            //PathHelper.SetDirectoryCompression(BotLogFolder);
            //PathHelper.SetDirectoryCompression(LogFolder);
        }


        public string GetPluginWorkingFolder(string pluginId)
            => InitSubFolder(AlgoWorkingFolder, PathHelper.Escape(pluginId));

        public string GetPluginLogsFolder(string pluginId)
            => InitSubFolder(BotLogFolder, PathHelper.Escape(pluginId));


        private static string InitSubFolder(string basePath, string subPath)
        {
            var folderPath = Path.Combine(basePath, subPath);
            try
            {
                PathHelper.EnsureDirectoryCreated(folderPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Can't create directory '{folderPath}'");
            }
            return folderPath;
        }
    }
}
