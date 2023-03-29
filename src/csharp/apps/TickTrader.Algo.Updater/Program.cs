using System;
using System.Diagnostics;
using System.IO;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.Algo.Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                ExitWithCode(UpdateExitCodes.InvalidArgs);

            if (!Enum.TryParse<UpdateAppTypes>(args[0], out var appType))
                ExitWithCode(UpdateExitCodes.InvalidAppType);
            var exeFileName = UpdateHelper.GetAppExeFileName(appType);
            if (string.IsNullOrEmpty(exeFileName))
                ExitWithCode(UpdateExitCodes.UnexpectedAppType);

            var installPath = args[1];
            if (!Directory.Exists(installPath))
                ExitWithCode(UpdateExitCodes.AppPathNotFound);

            var currentFolder = InstallPathHelper.GetCurrentVersionFolder(installPath);
            if (!Directory.Exists(currentFolder))
                ExitWithCode(UpdateExitCodes.CurrentVersionNotFound);
            var updateFolder = InstallPathHelper.GetUpdateVersionFolder(installPath);
            if (!Directory.Exists(updateFolder))
                ExitWithCode(UpdateExitCodes.UpdateVersionNotFound);
            if (!File.Exists(Path.Combine(updateFolder, exeFileName)))
                ExitWithCode(UpdateExitCodes.UpdateVersionMissingExe);

            // Wait for current process to stop. PID - ?. Service stop in case of server - ?

            var fallbackFolder = InstallPathHelper.GetFallbackVersionFolder(installPath);
            if (Directory.Exists(fallbackFolder))
                Directory.Delete(fallbackFolder, true);

            Directory.Move(currentFolder, fallbackFolder);
            Directory.Move(updateFolder, currentFolder);

            switch (appType)
            {
                case UpdateAppTypes.Terminal:
                    var exePath = Path.Combine(currentFolder, exeFileName);
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                    break;
                case UpdateAppTypes.Server:
                    // Start service. ServiceId - ?
                    break;
            }
        }


        private static void ExitWithCode(UpdateExitCodes code) => Environment.Exit((int)code);
    }
}