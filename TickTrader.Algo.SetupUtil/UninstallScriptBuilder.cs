using System;
using System.IO;
using System.Text;

namespace TickTrader.Algo.SetupUtil
{
    class UninstallScriptBuilder
    {
        private string _appDir;

        public UninstallScriptBuilder UseAppDir(string appDir)
        {
            _appDir = appDir;
            return this;
        }

        public string Build()
        {
            if (!Directory.Exists(_appDir))
                Console.WriteLine($"Directory {_appDir} not found");

            Console.WriteLine("Creating uninstall script...");
            Console.WriteLine("Builder uses the following parameters");
            Console.WriteLine($"  AppDir: {_appDir}");

            var nsisScript = new StringBuilder();
            CleanUpInstallDir(new DirectoryInfo(_appDir), nsisScript, "$INSTDIR");

            return nsisScript.ToString();
        }

        private void CleanUpInstallDir(DirectoryInfo appDirectory, StringBuilder nsisScript, string instDirFullPath)
        {
            foreach (var file in appDirectory.GetFiles())
            {
                nsisScript.AppendLine($"\tDelete \"{instDirFullPath}\\{file.Name}\"");
            }

            foreach (var subDir in appDirectory.GetDirectories())
            {
                CleanUpInstallDir(subDir, nsisScript, $"{instDirFullPath}\\{subDir.Name}");
            }

            nsisScript.AppendLine($"\tRMDir \"{instDirFullPath}\\\"");
            nsisScript.AppendLine();
        }
    }
}
