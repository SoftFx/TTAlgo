using System;
using System.Diagnostics;
using System.IO;

namespace TickTrader.Algo.AppCommon.Update
{
    public static class UpdateHelper
    {
        public const string UpdaterFileName = "TickTrader.Algo.Updater.exe";
        public const string TerminalFileName = "TickTrader.AlgoTerminal.exe";
        public const string ServerFileName = "TickTrader.AlgoServer.exe";
        public const int UpdateFailTimeout = 1_000;
        public const int ShutdownTimeout = 30_000;


        public static string GetAppExeFileName(UpdateAppTypes appType)
        {
            return appType switch
            {
                UpdateAppTypes.Terminal => "TickTrader.AlgoTerminal.exe",
                UpdateAppTypes.Server => "TickTrader.AlgoServer.exe",
                _ => string.Empty
            };
        }

        public static string GetUpdateBinFolder(string updatePath) => Path.Combine(updatePath, "update");


        public static UpdateErrorCodes RunUpdate(string updatePath, UpdaterParams updParams)
        {
            var updaterExe = Path.Combine(updatePath, UpdaterFileName);
            var startInfo = new ProcessStartInfo(updaterExe) { UseShellExecute = true, WorkingDirectory = updatePath };
            updParams.SaveAsEnvVars(startInfo.Environment);

            var proc = Process.Start(updaterExe);
            proc.WaitForExit(UpdateFailTimeout); // wait in case of any issues with update

            if (proc.HasExited)
                return (UpdateErrorCodes)proc.ExitCode;

            return UpdateErrorCodes.NoError;
        }
    }
}
