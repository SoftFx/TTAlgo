using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
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

        private readonly string _workDir = Directory.GetCurrentDirectory();


        public UpdateErrorCodes ErrorCode { get; private set; }

        public bool HasError => ErrorCode != UpdateErrorCodes.NoError;

        public UpdateState State { get; private set; }

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
                State = UpdateHelper.LoadUpdateState(_workDir);
            }
            catch (Exception ex)
            {
                ErrorCode = UpdateErrorCodes.InitError;
                Log.Error(ex, "Failed to load UpdateState");
            }

            if (!HasError && State != null)
            {
                try
                {
                    ErrorCode = InitInternal();
                    if (HasError)
                        LogError($"Init failed with error code = {ErrorCode}");
                }
                catch (Exception ex)
                {
                    ErrorCode = UpdateErrorCodes.InitError;
                    LogError("Init failed with generic error", ex);
                }

                if (HasError)
                {
                    State.InitErrorCode = (int)ErrorCode;
                    State.SetStatus(UpdateStatusCodes.InitFailed);
                    TrySaveState();
                }
            }
        }


        public Task RunUpdateAsync() => Task.Run(() => RunUpdate());


        private UpdateErrorCodes InitInternal()
        {
            var updParams = State.Params;
            if (updParams == null)
                return UpdateErrorCodes.MissingParams;

            if (State.Status != UpdateStatusCodes.Pending)
                return UpdateErrorCodes.IncorrectStatus;

            AppType = updParams.AppType;
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

        private void TrySaveState()
        {
            try
            {
                UpdateHelper.SaveUpdateState(_workDir, State);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save UpdateState");
            }
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
                State.SetStatus(UpdateStatusCodes.UpdateError);
            }

            TrySaveState();
            UpdateObserver?.OnCompleted();
        }

        private async Task RunUpdateInternal()
        {
            UpdateStatus("Waiting for app to stop...");
            var stopSuccess = await StopApp();
            if (!stopSuccess)
            {
                State.SetStatus(UpdateStatusCodes.UpdateError);
                UpdateStatus("Starting old version...");
                _ = await StartApp();
                return;
            }
            UpdateStatus("App stopped");

            var rollbackFolder = InstallPathHelper.GetRollbackVersionFolder(InstallPath);
            if (Directory.Exists(rollbackFolder))
            {
                UpdateStatus("Removing old rollback version...");
                await SafeFSAction("Remove rollback version", () => Directory.Delete(rollbackFolder, true));
            }

            await SafeFSAction("Move version current->rollback", () => Directory.Move(CurrentBinFolder, rollbackFolder));
            UpdateStatus("Moved current version to rollback");

            var copySuccess = false;
            try
            {
                UpdateStatus("Copying new version files...");
                PathHelper.CopyDirectory(UpdateBinFolder, CurrentBinFolder, true, false);
                UpdateStatus("Finished copying new files");
                copySuccess = true;
            }
            catch (Exception ex)
            {
                LogError("Copy new version failed", ex);
            }

            var startSuccess = false;
            if (copySuccess)
            {
                UpdateStatus("Starting new version...");
                startSuccess = await StartApp();
            }

            if (startSuccess)
                State.SetStatus(UpdateStatusCodes.Completed);

            if (!copySuccess || !startSuccess)
            {
                try
                {
                    UpdateStatus("Perfoming rollback...");
                    await SafeFSAction("Delete new version", () => Directory.Delete(CurrentBinFolder, true));
                    await SafeFSAction("Restore version from rollback", () => Directory.Move(rollbackFolder, CurrentBinFolder));
                    UpdateStatus("Rollback finished");

                    UpdateStatus("Starting rollback version...");
                    var rollbackStarted = await StartApp();
                    State.SetStatus(rollbackStarted ? UpdateStatusCodes.RollbackCompleted : UpdateStatusCodes.RollbackFailed);
                }
                catch (Exception ex)
                {
                    LogError("Rollback failed", ex);
                    State.SetStatus(UpdateStatusCodes.RollbackFailed);
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

        private void LogError(string msg, Exception ex = null)
        {
            Log.Error(ex, msg);
            State?.UpdateErrors?.Add(msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }

        private async Task SafeFSAction(string actionName, Action action)
        {
            // Windows doesn't always releases files immediately after process is killed
            await Task.Delay(1000); // Initial delay
            Exception error = null;
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    error = ex;
                    Log.Error(ex, actionName);
                }

                await Task.Delay(7000); // delay between attempts
            }
            throw error;
        }


        private static List<Process> GetAllProcessesForFolder(string processNamePrefix, string folderPath)
            => !string.IsNullOrEmpty(processNamePrefix)
                ? Process.GetProcesses().Where(p => p.ProcessName.StartsWith(processNamePrefix) && IsProcessMainFileInFolder(p, folderPath)).ToList()
                : Process.GetProcesses().Where(p => p.Id != 0 && p.Id != 4 && IsProcessMainFileInFolder(p, folderPath)).ToList();

        private static bool IsProcessMainFileInFolder(Process p, string folderPath)
            => p.MainModule?.FileName?.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
