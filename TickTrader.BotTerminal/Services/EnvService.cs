using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class EnvService
    {
        private Logger _logger;


        public static EnvService Instance { get; } = new EnvService();


        public string AppFolder { get; }

        public string RedistFolder => Path.Combine(AppFolder, "Redist");

        public string FeedHistoryCacheFolder { get; }

        public string ApplicationName { get; }

        public string BotLogFolder { get; }

        public string LogFolder { get; }

        public string JournalFolder { get; }

        public string AlgoRepositoryFolder { get; }

        public string AlgoExtFolder { get; }

        public string AlgoCommonRepositoryFolder { get; }

        public string AlgoWorkingFolder { get; }

        public string AppDataFolder { get; }

        public string AppLockFilePath { get; }

        public IObjectStorage UserDataStorage { get; }

        public IObjectStorage ProtectedUserDataStorage { get; }

        public string UserProfilesFolder { get; }

        public string ProfilesCacheFolder { get; }

        public IObjectStorage ProfilesCacheStorage { get; }


        private EnvService()
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            ApplicationName = "BotTrader";

            AppFolder = AppDomain.CurrentDomain.BaseDirectory;
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDocumentsFolder = Path.Combine(myDocumentsFolder, ApplicationName);

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                // ------- Click Once Mode -------------

                BotLogFolder = Path.Combine(appDocumentsFolder, "BotLogs");
                LogFolder = Path.Combine(appDocumentsFolder, "Logs");
                JournalFolder = Path.Combine(appDocumentsFolder, "Journals");
                AlgoRepositoryFolder = Path.Combine(appDocumentsFolder, "AlgoRepository");
                AlgoExtFolder = Path.Combine(appDocumentsFolder, "AlgoExt");
                AlgoCommonRepositoryFolder = null;
                AlgoWorkingFolder = Path.Combine(appDocumentsFolder, "AlgoData");
                FeedHistoryCacheFolder = Path.Combine(appDocumentsFolder, "QuoteCache");
                AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                AppLockFilePath = Path.Combine(AppDataFolder, "applock");
                UserProfilesFolder = Path.Combine(appDocumentsFolder, "Profiles");
                ProfilesCacheFolder = Path.Combine(UserProfilesFolder, "Cache");
            }
            else
            {
                // ------- Portable Mode -------------

                BotLogFolder = Path.Combine(AppFolder, "BotLogs");
                LogFolder = Path.Combine(AppFolder, "Logs");
                JournalFolder = Path.Combine(AppFolder, "Journal");
                AlgoRepositoryFolder = Path.Combine(AppFolder, "AlgoRepository");
                AlgoExtFolder = Path.Combine(AppFolder, "AlgoExt");
                AlgoCommonRepositoryFolder = Path.Combine(appDocumentsFolder, "AlgoRepository");
                AlgoWorkingFolder = Path.Combine(AppFolder, "AlgoData");
                FeedHistoryCacheFolder = Path.Combine(AppFolder, "FeedCache");
                AppDataFolder = Path.Combine(AppFolder, "Settings");
                AppLockFilePath = Path.Combine(AppFolder, "applock");
                UserProfilesFolder = Path.Combine(AppFolder, "Profiles");
                ProfilesCacheFolder = Path.Combine(UserProfilesFolder, "Cache");
            }

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

            // This is required for normal Algo plugin execution. Do not change working folder elsewhere!
            Directory.SetCurrentDirectory(AlgoWorkingFolder);

            UserDataStorage = new XmlObjectStorage(new FolderBinStorage(AppDataFolder));
            ProtectedUserDataStorage = new XmlObjectStorage(new SecureStorageLayer(new FolderBinStorage(AppDataFolder)));
            ProfilesCacheStorage = new XmlObjectStorage(new FolderBinStorage(ProfilesCacheFolder));
        }


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
