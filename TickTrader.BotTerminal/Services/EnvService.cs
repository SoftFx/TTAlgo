using NLog;
using System;
using System.IO;

namespace TickTrader.BotTerminal
{
    internal class EnvService
    {
        private static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private EnvService()
        {
            ApplicationName = "AlgoTerminal";

            AppFolder = AppDomain.CurrentDomain.BaseDirectory;
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDocumentsFolder = Path.Combine(myDocumentsFolder, ApplicationName);

            //if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            //{
            //    // ------- Click Once Mode -------------

            //    BotLogFolder = Path.Combine(appDocumentsFolder, "BotLogs");
            //    LogFolder = Path.Combine(appDocumentsFolder, "Logs");
            //    JournalFolder = Path.Combine(appDocumentsFolder, "Journals");
            //    AlgoRepositoryFolder = Path.Combine(appDocumentsFolder, "AlgoRepository");
            //    AlgoExtFolder = Path.Combine(appDocumentsFolder, "AlgoExt");
            //    AlgoCommonRepositoryFolder = null;
            //    AlgoWorkingFolder = Path.Combine(appDocumentsFolder, "AlgoData");
            //    FeedHistoryCacheFolder = Path.Combine(appDocumentsFolder, "QuoteCache");
            //    CustomFeedCacheFolder = Path.Combine(appDocumentsFolder, "CustomQuoteCache");
            //    AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //    AppLockFilePath = Path.Combine(AppDataFolder, "applock");
            //    UserProfilesFolder = Path.Combine(appDocumentsFolder, "Profiles");
            //    ProfilesCacheFolder = Path.Combine(UserProfilesFolder, "Cache");
            //    BacktestResultsFolder = Path.Combine(appDocumentsFolder, "Backtest Results");
            //}
            //else
            //{
            // ------- Portable Mode -------------

            BotLogFolder = Path.Combine(AppFolder, "BotLogs");
            LogFolder = Path.Combine(AppFolder, "Logs");
            JournalFolder = Path.Combine(AppFolder, "Journal");
            AlgoRepositoryFolder = Path.Combine(AppFolder, "AlgoRepository");
            AlgoExtFolder = Path.Combine(AppFolder, "AlgoExt");
            AlgoCommonRepositoryFolder = Path.Combine(appDocumentsFolder, "AlgoRepository");
            AlgoWorkingFolder = Path.Combine(AppFolder, "AlgoData");
            FeedHistoryCacheFolder = Path.Combine(AppFolder, "FeedCache");
            CustomFeedCacheFolder = Path.Combine(AppFolder, "FeedCache", "CustomFeed");
            AppDataFolder = Path.Combine(AppFolder, "Settings");
            AppLockFilePath = Path.Combine(AppFolder, "applock");
            UserProfilesFolder = Path.Combine(AppFolder, "Profiles");
            ProfilesCacheFolder = Path.Combine(UserProfilesFolder, "Cache");
            BacktestResultsFolder = Path.Combine(AppFolder, "BacktestResults");
            //}

            try
            {
                EnsureFolder(AlgoRepositoryFolder);
                EnsureFolder(AlgoExtFolder);
                EnsureFolder(AlgoCommonRepositoryFolder);
                EnsureFolder(AlgoWorkingFolder);
                EnsureFolder(BotLogFolder);
                EnsureFolder(LogFolder);
                EnsureFolder(JournalFolder);
                EnsureFolder(FeedHistoryCacheFolder);
                EnsureFolder(AppDataFolder);
                EnsureFolder(UserProfilesFolder);
                EnsureFolder(ProfilesCacheFolder);
                EnsureFolder(BacktestResultsFolder);

                // This is required for normal Algo plugin execution. Do not change working folder elsewhere!
                Directory.SetCurrentDirectory(AlgoWorkingFolder);

                UserDataStorage = new XmlObjectStorage(new FolderBinStorage(AppDataFolder));
                ProtectedUserDataStorage = new XmlObjectStorage(new SecureStorageLayer(new FolderBinStorage(AppDataFolder)));
                ProfilesCacheStorage = new XmlObjectStorage(new FolderBinStorage(ProfilesCacheFolder));

                InitFailed = false;
            }
            catch (UnauthorizedAccessException)
            {
                InitFailed = true;
            }
        }

        private static EnvService instance = new EnvService();
        public static EnvService Instance { get { return instance; } }

        public bool InitFailed { get; private set; }
        public string AppFolder { get; private set; }
        public string RedistFolder { get { return Path.Combine(AppFolder, "Redist"); } }
        public string FeedHistoryCacheFolder { get; private set; }
        public string CustomFeedCacheFolder { get; private set; }
        public string ApplicationName { get; private set; }
        public string BotLogFolder { get; private set; }
        public string LogFolder { get; private set; }
        public string JournalFolder { get; private set; }
        public string AlgoRepositoryFolder { get; private set; }
        public string AlgoExtFolder { get; private set; }
        public string AlgoCommonRepositoryFolder { get; private set; }
        public string AlgoWorkingFolder { get; private set; }
        public string AppDataFolder { get; private set; }
        public string AppLockFilePath { get; private set; }
        public string BacktestResultsFolder { get; private set; }
        public IObjectStorage UserDataStorage { get; private set; }
        public IObjectStorage ProtectedUserDataStorage { get; private set; }
        public IObjectStorage ProfilesCacheStorage { get; private set; }
        public string UserProfilesFolder { get; }
        public string ProfilesCacheFolder { get; }

        public void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (IOException ex)
            {
                _logger.Error("Cannot create directory {0}: {1}", folderPath, ex.Message);
            }
        }
    }
}
