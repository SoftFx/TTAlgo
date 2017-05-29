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
        private Logger logger;
        private EnvService()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
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

            // This is required for normal Algo plugin execution. Do not change working folder elsewhere!
            Directory.SetCurrentDirectory(AlgoWorkingFolder);

            UserDataStorage = new XmlObjectStorage(new FolderBinStorage(AppDataFolder));
            ProtectedUserDataStorage = new XmlObjectStorage(new SecureStorageLayer(new FolderBinStorage(AppDataFolder)));
        }

        private static EnvService instance = new EnvService();
        public static EnvService Instance { get { return instance; } }

        public string AppFolder { get; private set; }
        public string RedistFolder { get { return Path.Combine(AppFolder, "Redist"); } }
        public string FeedHistoryCacheFolder { get; private set; }
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
        public IObjectStorage UserDataStorage { get; private set; }
        public IObjectStorage ProtectedUserDataStorage { get; private set; }

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
                logger.Error("Cannot create directory {0}: {1}", folderPath, ex.Message);
            }
        }
    }
}
