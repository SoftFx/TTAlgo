using System;
using System.IO;
using System.Windows;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.Configurator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static string DataPath { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;


        protected override void OnStartup(StartupEventArgs e)
        {
            var dataPath = AppDomain.CurrentDomain.BaseDirectory;

            if (CheckIfInstallPattern(dataPath))
            {
                // resolve server data path
                var appInfo = AppInfoResolved.Create(new ResolveAppInfoRequest
                {
                    BinFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."),
                    IgnorePortableFlag = true,
                });

                if (!appInfo.HasError && Directory.Exists(appInfo.DataPath))
                    dataPath = Path.Combine(appInfo.DataPath, "Configurator");
            }

            PathHelper.EnsureDirectoryCreated(dataPath);
            DataPath = dataPath;
            Directory.SetCurrentDirectory(dataPath); // used in nlog.config

            base.OnStartup(e);
        }


        private static bool CheckIfInstallPattern(string binPath)
        {
            binPath = binPath.TrimEnd(Path.DirectorySeparatorChar);

            // pattern 1: {server_binaries}/Configurator
            var candidate1 = Path.GetFullPath(Path.Combine(binPath, "..", "Configurator", ""));
            if (string.Equals(binPath, candidate1, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
