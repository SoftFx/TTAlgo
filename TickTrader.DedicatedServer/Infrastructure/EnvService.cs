using System;
using System.IO;

namespace TickTrader.DedicatedServer.Infrastructure
{
    public class EnvService
    {
        public EnvService()
        {
            AppFolder = AppDomain.CurrentDomain.BaseDirectory;
            BotLogFolder = Path.Combine(AppFolder, "BotLogs");
            LogFolder = Path.Combine(AppFolder, "Logs");
            AlgoRepositoryFolder = Path.Combine(AppFolder, "AlgoRepository");
            AlgoExtFolder = Path.Combine(AppFolder, "AlgoExt");
            AlgoWorkingFolder = Path.Combine(AppFolder, "AlgoData");
            FeedHistoryCacheFolder = Path.Combine(AppFolder, "FeedCache");
            AppDataFolder = Path.Combine(AppFolder, "Settings");
            EnsureFolder(AlgoRepositoryFolder);
            EnsureFolder(AlgoExtFolder);
            EnsureFolder(AlgoWorkingFolder);
            EnsureFolder(BotLogFolder);
            EnsureFolder(LogFolder);
            EnsureFolder(FeedHistoryCacheFolder);
            EnsureFolder(AppDataFolder);
        }

        public string AppFolder { get; private set; }
        public string FeedHistoryCacheFolder { get; private set; }
        public string BotLogFolder { get; private set; }
        public string LogFolder { get; private set; }
        public string AlgoRepositoryFolder { get; private set; }
        public string AlgoExtFolder { get; private set; }
        public string AlgoWorkingFolder { get; private set; }
        public string AppDataFolder { get; private set; }

        public void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (IOException)
            {
                // do nothing
            }
        }

    }

    
}
