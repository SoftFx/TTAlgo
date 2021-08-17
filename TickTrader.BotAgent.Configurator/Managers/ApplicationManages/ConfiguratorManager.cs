using System;
using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public static class ConfiguratorManager
    {
        private const string BackupFolerName = "Backups";


        public static string Directory => Environment.CurrentDirectory;

        public static string BackupFolder { get; }


        static ConfiguratorManager()
        {
            BackupFolder = Path.Combine(Directory, BackupFolerName);

            System.IO.Directory.CreateDirectory(BackupFolder);
        }
    }
}
