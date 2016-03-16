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
        public EnvService()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            AlgoRepositoryFolder = Path.Combine(appDir, "AlgoRepository");
            FeedHistoryCacheFolder = Path.Combine(appDir, "FeedCache");

            EnsureFolder(AlgoRepositoryFolder);

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            UserDataStorage = new XmlObjectStorage(new FolderBinStorage(appDataFolder));
            ProtectedUserDataStorage = new XmlObjectStorage(new SecureStorageLayer(new FolderBinStorage(appDataFolder)));
        }

        private static EnvService instance = new EnvService();
        public static EnvService Instance { get { return instance; } }

        public string FeedHistoryCacheFolder { get; private set; }
        public string AlgoRepositoryFolder { get; private set; }
        public IObjectStorage UserDataStorage { get; private set; }
        public IObjectStorage ProtectedUserDataStorage { get; private set; }

        private void EnsureFolder(string folderPath)
        {
            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine("Cannot create directory " + folderPath + ": " + ex.Message);
            }
        }
    }
}
