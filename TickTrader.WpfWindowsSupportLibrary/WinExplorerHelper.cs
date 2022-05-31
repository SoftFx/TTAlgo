using System.Diagnostics;
using System.IO;

namespace TickTrader.WpfWindowsSupportLibrary
{
    public static class WinExplorerHelper
    {
        public static void ShowFolder(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            Process.Start(new ProcessStartInfo { FileName = dirPath, UseShellExecute = true });
        }
    }
}
