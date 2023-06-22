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
    public interface IUpdateObserver
    {
        void OnStatusUpdated(string msg);

        bool KillQuestionCallback(string msg);

        void OnCompleted();
    }


    public class UpdateContext
    {
        private const int KillQuestionDelayCnt = 100;
        private const int SuccessStartTimeout = 10000;


        public UpdateErrorCodes ErrorCode { get; private set; }

        public bool HasError => ErrorCode != UpdateErrorCodes.NoError;

        public Exception ErrorDetails { get; private set; }

        public UpdateAppTypes AppType { get; private set; }

        public string ExeFileName { get; private set; }

        public string InstallPath { get; private set; }

        public string CurrentBinFolder { get; private set; }

        public string UpdateBinFolder { get; private set; }

        public IUpdateObserver UpdateObserver { get; set; }


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

        private async Task RunUpdate()
        {
            try
            {
                await RunUpdateInternal();
            }
            catch (Exception ex)
            {
                LogError("Unexpected update error", ex);
            }

            UpdateObserver?.OnCompleted();
        }

        private async Task RunUpdateInternal()
        {
            UpdateStatus("Waiting for app to stop...");
            var stopSuccess = await StopApp();
            if (!stopSuccess)
            {
                UpdateStatus("Starting old version...");
                _ = await StartApp();
                return;
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

            var startSuccess = true;
            if (copySuccess)
            {
                UpdateStatus("Starting new version...");
                startSuccess = await StartApp();
            }

            if (!copySuccess || !startSuccess)
            {
                try
                {
                    UpdateStatus("Perfoming rollback...");
                    await Task.Delay(1000); // wait for files to release
                    Directory.Delete(CurrentBinFolder, true);
                    Directory.Move(rollbackFolder, CurrentBinFolder);
                    UpdateStatus("Rollback finished");

                    UpdateStatus("Starting rollback version...");
                    _ = await StartApp();
                }
                catch (Exception ex)
                {
                    LogError("Rollback failed", ex);
                }
            }
        }

        private async Task<bool> StopApp()
        {
            try
            {
                var processesToStop = GetAllProcessesForFolder("TickTrader.Algo", CurrentBinFolder);

                var stopTask = AppType switch
                {
                    UpdateAppTypes.Terminal => StopTerminal(processesToStop),
                    UpdateAppTypes.Server => StopServer(processesToStop),
                    _ => throw new ArgumentException($"Invalid AppType: {AppType}"),
                };
                await stopTask;
                return true;
            }
            catch (Exception ex)
            {
                LogError("StopApp failed", ex);
                return false;
            }
        }

        private async Task StopTerminal(List<Process> processesToStop)
        {
            if (processesToStop.Count != 0)
            {
                var cnt = 0;
                while (processesToStop.Any(p => !p.HasExited))
                {
                    cnt++;
                    await Task.Delay(100);
                    if (cnt % KillQuestionDelayCnt == 0 && UpdateObserver != null)
                    {
                        var msgBuilder = new StringBuilder();
                        msgBuilder.AppendLine("Waiting for stop of process: ");
                        foreach (var p in processesToStop.Where(p => !p.HasExited)) msgBuilder.AppendLine($"{p.ProcessName} ({p.Id})");
                        msgBuilder.AppendLine("Kill now?");

                        var killNowFlag = UpdateObserver.KillQuestionCallback(msgBuilder.ToString());
                        if (killNowFlag)
                        {
                            foreach (var p in processesToStop)
                            {
                                if (!p.HasExited)
                                    p.Kill();
                            }
                        }
                    }
                }
            }
        }

        private async Task StopServer(List<Process> processesToStop)
        {
            // Wait for StopService. ServiceId - ?
        }

        private async Task<bool> StartApp()
        {
            try
            {
                var startTask = AppType switch
                {
                    UpdateAppTypes.Terminal => StartTerminal(),
                    UpdateAppTypes.Server => StartServer(),
                    _ => throw new ArgumentException($"Invalid AppType: {AppType}"),
                };
                return await startTask;
            }
            catch (Exception ex)
            {
                LogError("StartApp failed", ex);
                return false;
            }
        }

        private async Task<bool> StartTerminal()
        {
            var exePath = Path.Combine(CurrentBinFolder, ExeFileName);
            var p = Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
            await Task.WhenAny(p.WaitForExitAsync(), Task.Delay(SuccessStartTimeout));
            if (p.HasExited && p.ExitCode != 0)
            {
                UpdateStatus($"Start failed. Exit code: {p.ExitCode}");
                return false;
            }
            return true;
        }

        private async Task<bool> StartServer()
        {
            // Start service. ServiceId - ?
            return true;
        }

        private void UpdateStatus(string msg)
        {
            Log.Information(msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }

        private void LogError(string msg, Exception ex)
        {
            Log.Error(ex, msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }


        private static List<Process> GetAllProcessesForFolder(string processNamePrefix, string folderPath)
            => !string.IsNullOrEmpty(processNamePrefix)
                ? Process.GetProcesses().Where(p => p.ProcessName.StartsWith(processNamePrefix) && IsProcessMainFileInFolder(p, folderPath)).ToList()
                : Process.GetProcesses().Where(p => p.Id != 0 && p.Id != 4 && IsProcessMainFileInFolder(p, folderPath)).ToList();

        private static bool IsProcessMainFileInFolder(Process p, string folderPath)
            => p.MainModule?.FileName?.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
