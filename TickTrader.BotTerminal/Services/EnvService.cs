using NLog;
using System;
using System.IO;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class EnvService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly Lazy<string> _botLogFolder, _logFolder, _journalFolder,
            _algoRepoFolder, _algoCommonRepoFolder, _algoDataFolder, _appSettingsFolder,
            _feedCacheFolder, _customFeedCacheFolder, _profilesFolder, _profilesCacheFolder,
            _backtesterResFolder;
        private readonly Lazy<IObjectStorage> _userStorage, _protectedUserStorage, _profileStorage;


        public static EnvService Instance { get; } = new();


        public string ApplicationName { get; } = "AlgoTerminal";
        public string BinFolder { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public string AppFolder { get; }
        public string AppLockFilePath { get; }
        public bool InitFailed { get; }

        public string AppDataFolder => _appSettingsFolder.Value;
        public string AlgoRepositoryFolder => _algoRepoFolder.Value;
        public string AlgoCommonRepositoryFolder => _algoCommonRepoFolder.Value;
        public string AlgoWorkingFolder => _algoDataFolder.Value;
        public string BotLogFolder => _botLogFolder.Value;
        public string LogFolder => _logFolder.Value;
        public string JournalFolder => _journalFolder.Value;
        public string FeedHistoryCacheFolder => _feedCacheFolder.Value;
        public string CustomFeedCacheFolder => _customFeedCacheFolder.Value;
        public string UserProfilesFolder => _profilesFolder.Value;
        public string ProfilesCacheFolder => _profilesCacheFolder.Value;
        public string BacktestResultsFolder => _backtesterResFolder.Value;

        public IObjectStorage UserDataStorage => _userStorage.Value;
        public IObjectStorage ProtectedUserDataStorage => _protectedUserStorage.Value;
        public IObjectStorage ProfilesCacheStorage => _profileStorage.Value;


        private EnvService()
        {
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDocumentsFolder = Path.Combine(myDocumentsFolder, ApplicationName);

            //AppFolder = System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed
            //    ? appDocumentsFolder // Click Once Mode
            //    : BinFolder; // Portable Mode
            AppFolder = BinFolder;
            AppLockFilePath = Path.Combine(AppFolder, "applock");

            _appSettingsFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "Settings"), true);
            _algoRepoFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "AlgoRepository"), true);
            _algoCommonRepoFolder = AppFolder == appDocumentsFolder
                ? new Lazy<string>(() => null)
                : new Lazy<string>(() => InitSubFolder(appDocumentsFolder, "AlgoRepository"), true);
            _algoDataFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "AlgoData"), true);
            _botLogFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "BotLogs"), true);
            _logFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "Logs"), true);
            _journalFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "Journal"), true);
            _feedCacheFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "FeedCache"), true);
            _customFeedCacheFolder = new Lazy<string>(() => InitSubFolder(FeedHistoryCacheFolder, "CustomFeed"), true);
            _profilesFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "Profiles"), true);
            _profilesCacheFolder = new Lazy<string>(() => InitSubFolder(UserProfilesFolder, "Cache"), true);
            _backtesterResFolder = new Lazy<string>(() => InitSubFolder(AppFolder, "BacktestResults"), true);

            _userStorage = new Lazy<IObjectStorage>(() => new XmlObjectStorage(new FolderBinStorage(AppDataFolder)), true);
            _protectedUserStorage = new Lazy<IObjectStorage>(() => new XmlObjectStorage(new SecureStorageLayer(new FolderBinStorage(AppDataFolder))), true);
            _profileStorage = new Lazy<IObjectStorage>(() => new XmlObjectStorage(new FolderBinStorage(ProfilesCacheFolder)), true);

            try
            {
                EnsureFolder(AppFolder);

                InitFailed = false;
            }
            catch (UnauthorizedAccessException)
            {
                InitFailed = true;
            }
        }


        private static string InitSubFolder(string basePath, string subPath) => EnsureFolder(Path.Combine(basePath, subPath));

        private static string EnsureFolder(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                try
                {
                    PathHelper.EnsureDirectoryCreated(folderPath);
                }
                catch (IOException ex)
                {
                    _logger.Error("Cannot create directory {0}: {1}", folderPath, ex.Message);
                }
            }

            return folderPath;
        }
    }
}
