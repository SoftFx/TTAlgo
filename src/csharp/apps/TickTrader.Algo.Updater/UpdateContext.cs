using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Updater
{
    public class UpdateContext
    {
        public UpdateErrorCodes ErrorCode { get; set; }

        public bool HasError => ErrorCode != UpdateErrorCodes.NoError;

        public Exception ErrorDetails { get; set; }

        public UpdateAppTypes AppType { get; set; }

        public string ExeFileName { get; set; }

        public string InstallPath { get; set; }

        public string CurrentBinFolder { get; set; }

        public string UpdateBinFolder { get; set; }


        public event Action<string> StatusUpdated;


        public UpdateContext()
        {
            try
            {
                ErrorCode = InitInternal();
            }
            catch (Exception ex)
            {
                ErrorDetails = ex;
                ErrorCode = UpdateErrorCodes.InitError;
            }
        }


        public Task RunUpdateAsync() => Task.Run(() => RunUpdate());


        private UpdateErrorCodes InitInternal()
        {
            UpdaterParams updParams;
            try
            {
                updParams = UpdaterParams.ParseFromEnvVars(Environment.GetEnvironmentVariables());
            }
            catch (Exception ex)
            {
                ErrorDetails = ex;
                return UpdateErrorCodes.InitError;
            }
            if (updParams == null)
                return UpdateErrorCodes.InitError;

            if (!updParams.AppType.HasValue)
                return UpdateErrorCodes.InvalidAppType;
            AppType = updParams.AppType.Value;

            ExeFileName = UpdateHelper.GetAppExeFileName(AppType);
            if (string.IsNullOrEmpty(ExeFileName))
                return UpdateErrorCodes.UnexpectedAppType;

            if (!Directory.Exists(updParams.InstallPath))
                return UpdateErrorCodes.AppPathNotFound;
            InstallPath = updParams.InstallPath;

            CurrentBinFolder = InstallPathHelper.GetCurrentVersionFolder(InstallPath);
            if (!Directory.Exists(CurrentBinFolder))
                return UpdateErrorCodes.CurrentVersionNotFound;
            UpdateBinFolder = !string.IsNullOrEmpty(updParams.UpdatePath)
                ? updParams.UpdatePath
                : UpdateHelper.GetUpdateBinFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(UpdateBinFolder))
                return UpdateErrorCodes.UpdateVersionNotFound;
            if (!File.Exists(Path.Combine(UpdateBinFolder, ExeFileName)))
                return UpdateErrorCodes.UpdateVersionMissingExe;

            return UpdateErrorCodes.NoError;
        }

        public async Task RunUpdate()
        {
            var processesToStop = GetAllProcessesForFolder("TickTrader.Algo", CurrentBinFolder);

            UpdateStatus("Waiting for app to stop...");
            switch (AppType)
            {
                case UpdateAppTypes.Terminal:
                    if (processesToStop.Count != 0)
                    {
                        var cnt = 0;
                        while (processesToStop.Any(p => !p.HasExited))
                        {
                            if (cnt % 50 == 0)
                            {
                                var msgBuilder = new StringBuilder();
                                msgBuilder.AppendLine("Waiting for stop of process: ");
                                foreach (var p in processesToStop.Where(p => !p.HasExited)) msgBuilder.AppendLine($"{p.ProcessName} ({p.Id})");
                                UpdateStatus(msgBuilder.ToString());
                            }
                            cnt++;
                            await Task.Delay(100);
                        }
                    }
                    break;
                case UpdateAppTypes.Server:
                    // Wait for StopService. ServiceId - ?
                    break;
            }
            UpdateStatus("App stopped");

            var rollbackFolder = InstallPathHelper.GetRollbackVersionFolder(InstallPath);
            if (Directory.Exists(rollbackFolder))
            {
                UpdateStatus("Removing old rollback version...");
                Directory.Delete(rollbackFolder, true);
            }

            Directory.Move(CurrentBinFolder, rollbackFolder);
            UpdateStatus("Moved current version to rollback");

            var copySuccess = true;
            try
            {
                UpdateStatus("Copying new version files...");
                PathHelper.CopyDirectory(UpdateBinFolder, CurrentBinFolder, true, false);
                UpdateStatus("Finished copying new files");
            }
            catch (Exception ex)
            {
                copySuccess = false;
                LogError("Copy new version failed", ex);
            }

            if (!copySuccess)
            {
                try
                {
                    UpdateStatus("Perfoming rollback...");
                    Directory.Delete(CurrentBinFolder, true);
                    Directory.Move(rollbackFolder, CurrentBinFolder);
                    UpdateStatus("Rollback finished");
                }
                catch (Exception ex)
                {
                    LogError("Rollback failed", ex);
                }
            }

            StartApp();
        }

        private void StartApp()
        {
            switch (AppType)
            {
                case UpdateAppTypes.Terminal:
                    var exePath = Path.Combine(CurrentBinFolder, ExeFileName);
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                    break;
                case UpdateAppTypes.Server:
                    // Start service. ServiceId - ?
                    break;
            }
        }

        private void UpdateStatus(string msg)
        {
            Log.Information(msg);
            StatusUpdated?.Invoke(msg);
        }

        private void LogError(string msg, Exception ex)
        {
            Log.Error(ex, msg);
            StatusUpdated?.Invoke(msg);
        }


        private static List<Process> GetAllProcessesForFolder(string processNamePrefix, string folderPath)
            => !string.IsNullOrEmpty(processNamePrefix)
                ? Process.GetProcesses().Where(p => p.ProcessName.StartsWith(processNamePrefix) && IsProcessMainFileInFolder(p, folderPath)).ToList()
                : Process.GetProcesses().Where(p => p.Id != 0 && p.Id != 4 && IsProcessMainFileInFolder(p, folderPath)).ToList();

        private static bool IsProcessMainFileInFolder(Process p, string folderPath)
            => p.MainModule?.FileName?.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
