using System.Diagnostics;
using System.IO;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal class VsIntegration
    {
        public static void InstallVsPackage()
        {
            var vsixPath = Path.Combine(EnvService.Instance.RedistFolder, "TickTrader.Algo.VS.Package.vsix");
            if (File.Exists(vsixPath))
            {
                Process.Start(new ProcessStartInfo { FileName = vsixPath, UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Visual Studio plug-in is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
